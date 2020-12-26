using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AngularBlogCore.API.Models;
using AngularBlogCore.API.Responses;

namespace AngularBlogCore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticlesController : ControllerBase
    {
        private readonly masterContext _context;

        public ArticlesController(masterContext context)
        {
            _context = context;
        }

        // GET: api/Articles
        [HttpGet]
        public IEnumerable<Article> GetArticle()
        {
            return _context.Article;
        }

        [HttpGet("{page}/{pageSize}")]
        public IActionResult GetArticle(int page=1,int pageSize=5)
        {
            try
            {
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
                        Category = new CategoryResponse() { Id=x.Category.Id,Name = x.Category.Name}
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

        // GET: api/Articles/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetArticle([FromRoute] int id)
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

            return Ok(article);
        }

        // PUT: api/Articles/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutArticle([FromRoute] int id, [FromBody] Article article)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != article.Id)
            {
                return BadRequest();
            }

            _context.Entry(article).State = EntityState.Modified;

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

            _context.Article.Add(article);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetArticle", new { id = article.Id }, article);
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
    }
}