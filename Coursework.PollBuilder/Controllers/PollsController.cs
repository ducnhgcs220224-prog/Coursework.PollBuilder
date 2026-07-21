using Coursework.PollBuilder.Data;
using Coursework.PollBuilder.Data.Entities;
using Coursework.PollBuilder.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Coursework.PollBuilder.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PollsController : ControllerBase
    {
        private readonly PollBuilderDbContext _context;
        private readonly IHubContext<PollHub> _hubContext;

        public PollsController(PollBuilderDbContext context, IHubContext<PollHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // 1. TẠO POLL MỚI (POST /api/polls)
        [HttpPost]
        public async Task<IActionResult> CreatePoll([FromBody] CreatePollDto request)
        {
            var code = Guid.NewGuid().ToString().Substring(0, 6).ToUpper();

            var poll = new Poll
            {
                Id = Guid.NewGuid(),
                Code = code,
                Question = request.Question,
                Options = System.Text.Json.JsonSerializer.Serialize(request.Options)
            };

            _context.Polls.Add(poll);
            await _context.SaveChangesAsync();

            return Ok(new { Code = code });
        }

        // 2. LẤY THÔNG TIN POLL ĐỂ HIỂN THỊ (GET /api/polls/{code})
        [HttpGet("{code}")]
        public async Task<IActionResult> GetPoll(string code)
        {
            var poll = await _context.Polls.FirstOrDefaultAsync(p => p.Code == code);
            if (poll == null) return NotFound("Poll not found!");

            var options = System.Text.Json.JsonSerializer.Deserialize<List<string>>(poll.Options);

            return Ok(new
            {
                poll.Id,
                poll.Code,
                poll.Question,
                Options = options,
                poll.IsClosed // Đã trả về cờ này để Frontend biết mà ẩn nút Vote
            });
        }

        // 3. THỰC HIỆN BÌNH CHỌN (POST /api/polls/{code}/vote)
        [HttpPost("{code}/vote")]
        public async Task<IActionResult> Vote(string code, [FromBody] VoteDto request)
        {
            var poll = await _context.Polls.FirstOrDefaultAsync(p => p.Code == code);

            if (poll == null) return NotFound("Poll not found!");

            // CHẶN VOTE NẾU POLL ĐÃ ĐÓNG
            if (poll.IsClosed) return BadRequest("This poll has been closed. No further votes are accepted.");

            var alreadyVoted = await _context.Votes.AnyAsync(v => v.PollId == poll.Id && v.VoterToken == request.VoterToken);
            if (alreadyVoted) return BadRequest("You have already voted. No cheating allowed!");

            var vote = new Vote
            {
                Id = Guid.NewGuid(),
                PollId = poll.Id,
                OptionIndex = request.OptionIndex,
                VoterToken = request.VoterToken
            };

            _context.Votes.Add(vote);
            await _context.SaveChangesAsync();

            // REAL-TIME: Bắn tín hiệu "ReceiveVoteUpdate"
            await _hubContext.Clients.All.SendAsync("ReceiveVoteUpdate");

            return Ok("Vote cast successfully!");
        }

        // 4. LẤY KẾT QUẢ ĐỂ VẼ BIỂU ĐỒ (GET /api/polls/{code}/results)
        [HttpGet("{code}/results")]
        public async Task<IActionResult> GetResults(string code)
        {
            var poll = await _context.Polls.Include(p => p.Votes).FirstOrDefaultAsync(p => p.Code == code);
            if (poll == null) return NotFound("Poll not found!");

            var options = System.Text.Json.JsonSerializer.Deserialize<List<string>>(poll.Options);

            var results = options!.Select((opt, index) => new
            {
                OptionName = opt,
                VoteCount = poll.Votes.Count(v => v.OptionIndex == index)
            }).ToList();

            return Ok(results);
        }

        // 5. API MỚI: ĐÓNG BÌNH CHỌN (POST /api/polls/{code}/close)
        [HttpPost("{code}/close")]
        public async Task<IActionResult> ClosePoll(string code)
        {
            var poll = await _context.Polls.FirstOrDefaultAsync(p => p.Code == code);
            if (poll == null) return NotFound("Poll not found!");

            // Đổi trạng thái thành đã đóng
            poll.IsClosed = true;
            await _context.SaveChangesAsync();

            // Tùy chọn cực ngầu: Bắn tín hiệu Real-time để ai đang mở tab Vote tự động bị văng/thấy cảnh báo
            await _hubContext.Clients.All.SendAsync("ReceiveVoteUpdate");

            return Ok(new { message = "Poll closed successfully." });
        }

        #region DTOs
        public record CreatePollDto(string Question, List<string> Options);
        public record VoteDto(int OptionIndex, string VoterToken);
        #endregion
    }
}