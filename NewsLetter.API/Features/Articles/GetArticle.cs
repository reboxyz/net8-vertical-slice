using Carter;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using NewsLetter.API.Contracts.Articles;
using NewsLetter.API.Database;
using NewsLetter.API.Shared;

namespace NewsLetter.API.Features.Articles;

public static class GetArticle
{
    public class Query: IRequest<Result<ArticleResponse>>
    {
        public Guid Id { get; set; }
    }

    internal sealed class Handler : IRequestHandler<Query, Result<ArticleResponse>>
    {
        private readonly ApplicationDbContext _dbContext;

        public Handler(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<ArticleResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            var articleResponse = await _dbContext.Articles.AsNoTracking()
                .Where(a => a.Id == request.Id)
                .Select(a => new ArticleResponse
                {
                    Id = a.Id,
                    Title = a.Title,
                    Content = a.Content,
                    Tags = a.Tags,
                    CreatedOnUtc = a.CreatedOnUtc,
                    PublishedOnUtc = a.PublishedOnUtc
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (articleResponse is null)
            {
                return Result.Failure<ArticleResponse>(new Error("GetArticle.Null", "The article with specified was not found"));
            }

            return Result.Success<ArticleResponse>(articleResponse);
        }
    }
}

public class GetArticleEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/articles/{id}", [AllowAnonymous] async (Guid id, ISender sender) =>
        {
            var query = new GetArticle.Query { Id = id};
            var result = await sender.Send(query);

            if (result.IsFailure)
            {
                return Results.NotFound(result.Error);
            }

            return Results.Ok(result.Value);
        });
    }
}