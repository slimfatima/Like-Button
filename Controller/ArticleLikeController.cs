using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace LikeButtonAPI.Controllers
{
	[ApiController]
	[Route("api/articles/{articleId}/likes")]
	public class ArticleLikeController : ControllerBase
	{
		private readonly IDistributedCache _cache;

		public ArticleLikeController(IDistributedCache cache)
		{
			_cache = cache;
		}

		[HttpGet]
		public async Task<IActionResult> GetLikes(string articleId)
		{
			// Try to get the like count from Redis cache
			var cachedLikes = await _cache.GetStringAsync(articleId);
			if (cachedLikes != null)
			{
				return Ok(new { Likes = int.Parse(cachedLikes) });
			}

			// If not found in cache, return zero likes
			return Ok(new { Likes = 0 });
		}

		[HttpPost]
		public async Task<IActionResult> LikeArticle(string articleId)
		{
			// Get the current like count from Redis, or start at 0
			var cachedLikes = await _cache.GetStringAsync(articleId);
			int likes = cachedLikes != null ? int.Parse(cachedLikes) : 0;

			// Increment the like count
			likes += 1;

			// Update the cache with the new like count
			await _cache.SetStringAsync(articleId, likes.ToString(), new DistributedCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
			});

			return Ok(new { Likes = likes });
		}
	}
}
