using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

public class CacheSyncService : BackgroundService
{
    private readonly IDistributedCache _cache;
    private readonly ILikeRepository _repository;

    public CacheSyncService(IDistributedCache cache, ILikeRepository repository)
    {
        _cache = cache;
        _repository = repository;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var allArticles = await _repository.GetAllArticleIdsAsync();
            foreach (var articleId in allArticles)
            {
                var cachedLikes = await _cache.GetStringAsync(articleId);
                if (cachedLikes != null)
                {
                    int likes = int.Parse(cachedLikes);
                    await _repository.UpdateLikeCountAsync(articleId, likes);
                }
            }
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
