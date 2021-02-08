using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AngularBlogCore.API.Models;
using AngularBlogCore.API.Responses;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace AngularBlogCore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("CorsPolicy")]
    public class ArticlesController : ControllerBase
    {
        private readonly masterContext _context;

        public ArticlesController(masterContext context)
        {
            _context = context;
        }

        // GET: api/Articles
        [HttpGet]
        public  IActionResult GetArticle()
        {
            var articles= _context.Article.Include(a=>a.Category)
                .Include(b=>b.Comment).OrderByDescending(x => x.PublishData).ToList()
                .Select( y=>new ArticleResponse()
                {
                    Id = y.Id,
                    Title = y.Title,
                    Picture = y.Picture,
                    Category = new CategoryResponse() { Id = y.Category.Id,Name = y.Category.Name},
                    CommentCount = y.Comment.Count,
                    ViewCount = y.ViewCount,
                    PublishDate = y.PublishData
                        
                    
                });
            return Ok(articles);
        }



        [HttpGet("{page}/{pageSize}")]
        public IActionResult GetArticle(int page, int pageSize)
        {
            try
            {
                // System.Threading.Thread.Sleep(3000);
                IQueryable<Article> query;
                query = _context.Article.Include(x => x.Category).Include(y => y.Comment)
                    .OrderByDescending(z => z.PublishData);
                var totalCount = query.Count();
                var articleResponse = query.Skip((pageSize * (page - 1))).Take(pageSize).ToList().Select(x =>
                    new ArticleResponse()
                    {
                        Id = x.Id,
                        Title = x.Title,
                        ContentMain = x.ContentMain,
                        ContentSummary = x.ContentSummary,
                        Picture = x.Picture,
                        ViewCount = x.ViewCount,
                        CommentCount = x.Comment.Count,
                        Category = new CategoryResponse() { Id = x.Category.Id, Name = x.Category.Name }
                    });

                var result = new
                {
                    TotalCount = totalCount,
                    Articles = articleResponse
                };
                return Ok(result);
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("GetArticleWithCategory/{categoryId}/{page}/{pageSize}")]
        public IActionResult GetArticleWithCategory(int categoryId, int page, int pageSize)
        {
            IQueryable<Article> query = _context.Article.Include(x => x.Category)
                .Include(y => y.Comment).Where(z => z.CategoryId == categoryId).OrderByDescending(x => x.PublishData);
            var queryResult = ArticlePagination(query, page, pageSize);

            var result = new
            {
                Articles = queryResult.Item1,
                TotalCount = queryResult.Item2

            };
            return Ok(result);
        }

        [HttpGet]
        [Route("GetArticlesByMostView")]
        public IActionResult GetArticlesByMostView()
        {
            var articles = _context.Article.OrderByDescending(x => x.ViewCount).Take(5)
                .Select(y => new ArticleResponse()
                {
                    Title = y.Title,
                    Id = y.Id
                });
            return Ok(articles);
        }

        [HttpGet]
        [Route("GetArticlesArchive")]
        public IActionResult GetArticlesArchive()
        {
            var query = _context.Article.GroupBy(x => new { x.PublishData.Year, x.PublishData.Month })
                .Select(y => new
                {
                    year = y.Key.Year,
                    month = y.Key.Month,
                    count = y.Count(),
                    monthName = new DateTime(y.Key.Year, y.Key.Month, 1)
                        .ToString("MMMM", CultureInfo.CreateSpecificCulture("tr"))
                });
            return Ok(query);
        }

        [HttpGet]
        [Route("SearchArticles/{searchText}/{page}/{pageSize}")]
        public IActionResult SearchArticles(string searchText, int page, int pageSize)
        {
            IQueryable<Article> query;
            query = _context.Article.Include(x => x.Category).Include(y => y.Comment)
                .Where(z => z.Title.Contains(searchText)).OrderByDescending(f => f.PublishData);
            var resultQuery = ArticlePagination(query, page, pageSize);

            var result = new
            {
                Articles = resultQuery.Item1,
                TotalCount = resultQuery.Item2

            };
            return Ok(result);
        }


        [HttpGet]
        [Route("GetArticleArchiveList/{year}/{month}/{page}/{pageSize}")]
        public IActionResult GetArticleArchiveList(int year,int month,int page, int pageSize)
        {
            IQueryable<Article> query;
            query = _context.Article.Include(x => x.Category).Include(y => y.Comment)
                .Where(z => z.PublishData.Year == year && z.PublishData.Month == month)
                .OrderByDescending(f => f.PublishData);
            var resultQuery = ArticlePagination(query, page, pageSize);

            var result = new
            {
                Articles = resultQuery.Item1,
                TotalCount = resultQuery.Item2

            };
            return Ok(result);
           
        }


        // GET: api/Articles/5
        [HttpGet("{id}")]
        public IActionResult GetArticle([FromRoute] int id)
        {
            var article = _context.Article.Include(x => x.Category)
                .Include(y => y.Comment).FirstOrDefault(z => z.Id == id);
            ArticleResponse articleResponse = null;
            if (article != null)
            {
                articleResponse = new ArticleResponse()
                {
                    Id = article.Id,
                    Title = article.Title,
                    ContentSummary = article.ContentSummary,
                    ContentMain = article.ContentMain,
                    Picture = article.Picture,
                    PublishDate = article.PublishData,
                    ViewCount = article.ViewCount,
                    Category = new CategoryResponse() { Id = article.Category.Id, Name = article.Category.Name },
                    CommentCount = article.Comment.Count
                };
            }
            else
            {
                return NotFound("Makale Bulunmadı");
            }

            return Ok(articleResponse);
        }

        // PUT: api/Articles/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutArticle([FromRoute] int id, [FromBody] Article article)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Article tempArticle = await _context.Article.FindAsync(id);
            tempArticle.Id = id;
            tempArticle.Title = article.Title;
            tempArticle.ContentMain = article.ContentMain;
            tempArticle.ContentSummary = article.ContentSummary;
            tempArticle.CategoryId = article.Category.Id;
            tempArticle.Picture = article.Picture;
            
            _context.Entry(tempArticle).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ArticleExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Articles
        
        [HttpPost]
        public async Task<IActionResult> PostArticle([FromBody] Article article)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (article.Category != null)
            {
                article.CategoryId = article.Category.Id;
            }

            article.Category = null;
            article.ViewCount = 0;
            article.PublishData = DateTime.Now;
            _context.Article.Add(article);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: api/Articles/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteArticle([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var article = await _context.Article.FindAsync(id);
            if (article == null)
            {
                return NotFound();
            }

            _context.Article.Remove(article);
            await _context.SaveChangesAsync();

            return Ok(article);
        }

        private bool ArticleExists(int id)
        {
            return _context.Article.Any(e => e.Id == id);
        }

        public System.Tuple<IEnumerable<ArticleResponse>, int> ArticlePagination(IQueryable<Article> query, int page,
            int pageSize)
        {
            var totalCount = query.Count();
            var articleResponse = query.Skip((pageSize * (page - 1))).Take(pageSize).ToList().Select(x =>
                new ArticleResponse()
                {
                    Id = x.Id,
                    Title = x.Title,
                    ContentMain = x.ContentMain,
                    ContentSummary = x.ContentSummary,
                    Picture = x.Picture,
                    ViewCount = x.ViewCount,
                    CommentCount = x.Comment.Count,
                    Category = new CategoryResponse() { Id = x.Category.Id, Name = x.Category.Name }
                });

            return new System.Tuple<IEnumerable<ArticleResponse>, int>(articleResponse, totalCount);
        }

        [Route("ArticleViewCountUp/{id}")]
        [HttpGet()]
        public IActionResult ArticleViewCountUp(int id)
        {
            Article article = _context.Article.Find(id);
            article.ViewCount += 1;
            _context.SaveChanges();
            return Ok();
        }
        
        [HttpPost]
        [Route("SaveArticlePicture")]
        public async Task<IActionResult> SaveArticlePicture(IFormFile picture)
        {
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(picture.FileName);
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/articlePictures",fileName);

            using (var stream = new FileStream(path,FileMode.Create))
            {
                await picture.CopyToAsync(stream);
            }

            var result = new
            {
                path = "https://" + Request.Host + "/articlePictures/"+fileName
            };
            return  Ok(result);
        }
    }

    

}