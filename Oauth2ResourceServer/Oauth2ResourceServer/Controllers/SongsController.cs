using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Oauth2ResourceServer.Models;

namespace Oauth2ResourceServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SongsController : ControllerBase
    {
        private readonly Oauth2ResourceServerContext _context;
        private readonly ILogger<SongsController> _logger;
        private readonly IMemoryCache _memoryCache;

        public SongsController(
            Oauth2ResourceServerContext context,
            ILogger<SongsController> logger,
            IMemoryCache memoryCache)
        {
            _context = context;
            _logger = logger;
            _memoryCache = memoryCache;
        }

        // GET: api/Songs
        [HttpGet]
        [Route("LatestSong")]
        public IEnumerable<Song> GetLatestSong()
        {           
            return _context.Song;
        }

        // GET: api/Songs/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSong([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var song = await _context.Song.FindAsync(id);

            if (song == null)
            {
                return NotFound();
            }

            return Ok(song);
        }

        [HttpGet]
        [Route("MineSong")]
        public IActionResult GetMineSong([FromHeader(Name = "Authorization")] string authorizationToken)
        {
            var basicToken = authorizationToken + "";
            // lấy token từ header.
            var accessToken = basicToken.Replace("Basic ", "");
            // nên ktra lại token
            var credential = _memoryCache.Get<Credential>(accessToken);
            var songs = _context.Song.Where(s => s.MemberId == credential.AccountId).ToList();

            return Ok(songs);
        }

        // PUT: api/Songs/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSong([FromRoute] long id, [FromBody] Song song)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != song.Id)
            {
                return BadRequest();
            }

            _context.Entry(song).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SongExists(id))
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

        // POST: api/Songs
        [HttpPost]
        public async Task<IActionResult> PostSong([FromBody] Song song, [FromHeader(Name = "Authorization")] string authorizationToken)
        {
            var basicToken = authorizationToken + "";
            // lấy token từ header.
            var accessToken = basicToken.Replace("Basic ", "");
            // nên ktra lại token
            var credential = _memoryCache.Get<Credential>(accessToken);
            if (credential == null || !credential.IsValid())
            {
                return StatusCode(403);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            song.MemberId = credential.AccountId;
            _context.Song.Add(song);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSong", new { id = song.Id }, song);
        }

        // DELETE: api/Songs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSong([FromRoute] long id)
        {           
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var song = await _context.Song.FindAsync(id);
            if (song == null)
            {
                return NotFound();
            }

            _context.Song.Remove(song);
            await _context.SaveChangesAsync();

            return Ok(song);
        }

        private bool SongExists(long id)
        {
            return _context.Song.Any(e => e.Id == id);
        }
    }
}