using Carter;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using NewsLetter.API.Contracts.Articles;
using NewsLetter.API.Database;
using NewsLetter.API.Entities;
using NewsLetter.API.Shared;

namespace NewsLetter.API.Features.Articles;

public static class CreateArticle
{
    public class Command: IRequest<Result<Guid>>
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
    }

    public class Validator: AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Title).NotEmpty();
            RuleFor(c => c.Content).NotEmpty();
        }
    }

    internal sealed class Handler : IRequestHandler<Command, Result<Guid>>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IValidator<Command> _validator;

        public Handler(ApplicationDbContext dbContext, IValidator<Command> validator)
        {
            _dbContext = dbContext;
            _validator = validator;
        }

        public async Task<Result<Guid>> Handle(Command request, CancellationToken cancellationToken)
        {
            var validationResult = _validator.Validate(request);

            if (!validationResult.IsValid)
            {
                return Result.Failure<Guid>(new Error("CreateArticle.Validation", validationResult.ToString()));
            }

            var article = new Article 
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Content = request.Content,
                Tags = request.Tags,
                CreatedOnUtc = DateTime.UtcNow,
            };

            await _dbContext.AddAsync(article);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success<Guid>(article.Id);
        }
    }
}

public class CreateArticleEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/articles", [AllowAnonymous] async (CreateArticleRequest request, ISender sender) => 
        {
            var comand = request.Adapt<CreateArticle.Command>(); // field mapper
            var result = await sender.Send(comand);

            if (result.IsFailure)
            {
                return Results.BadRequest(result.Error);   
            }

            return Results.Ok(result.Value);
        });
    }
}