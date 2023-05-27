using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tgbotapi.Data;
using tgbotapi.Models;
using tgbotapi.Requests;

namespace tgbotapi.Controllers;

[ApiController]
[Route("[controller]")]
public class Controller: Microsoft.AspNetCore.Mvc.Controller
{
    private readonly Context _dbContext;
    
    public Controller(
            Context dbContext
        )
    {
        this._dbContext = dbContext;
    }
    
    // Users
    
    [HttpPost]
    [Route("api/register")]
    public async Task<IActionResult> RegisterUser(RegisterUserRequest request)
    {
        var users = _dbContext.Users.Where(u => u.ChatId == request.ChatId).Count();
        
        if (users > 0)
        {
            return BadRequest("User already exists");
        }
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            ChatId = request.ChatId
        };
        
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        return Ok(user);
    }
    
    // Subjects
    
    [HttpGet]
    [Route("api/{chatId}/subjects")]
    public async Task<IActionResult> GetUserSubjects([FromRoute] string chatId)
    {
        
        var user = await this.GetUserByChatId(chatId);
        
        var subjects = await _dbContext.Subjects.Where(s => s.UserId == user.Id).ToListAsync();
        return Ok(
            subjects
        );
    }
    
    [HttpGet]
    [Route("api/{chatId}/subjects/{id:guid}/events")]
    public async Task<IActionResult> GetUserSubjectEvents([FromRoute] string chatId, [FromRoute] Guid id)
    {
        
        var user = await this.GetUserByChatId(chatId);
        var subject = await this.GetSubject(user, id);
        
        var events = await _dbContext.Events.Where(e => e.SubjectId == subject.Id).ToListAsync();
        return Ok(
            events
        );
    }
    
    [HttpGet]
    [Route("api/{chatId}/subjects/summary")]
    public async Task<IActionResult> GetUserSubjectsSummary([FromRoute] string chatId, [FromRoute] Guid id)
    {

        var user = await this.GetUserByChatId(chatId);
        var subjects = await _dbContext.Subjects.Where(s => s.UserId == user.Id).ToListAsync();
        var subjectsCount = _dbContext.Subjects.Where(s => s.UserId == user.Id).Count();

        var response = new List<SubjectSummary>();

        foreach (var subject in subjects)
        {
            double total = 0;
            
            var events = await _dbContext.Events.Where(e => e.SubjectId == subject.Id).ToListAsync();
            
            foreach (var _event in events)
            {
                total += _event.Mark;
            }
            
            var tempSubjectSummary = new SubjectSummary(subject.Name, total);
            response.Add(tempSubjectSummary);
        }
        
        return Ok(
            response
        );
    }
    
    [HttpPost]
    [Route("api/{chatId}/subjects")]
    public async Task<IActionResult> CreateSubject([FromRoute] string chatId, CreateSubjectRequest request)
    {
        
        var user = await this.GetUserByChatId(chatId);

        var subject = new Subject
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Name = request.Name
        };
        
        _dbContext.Subjects.Add(subject);
        await _dbContext.SaveChangesAsync();

        return Ok(subject);
    }
    
    [HttpPut]
    [Route("api/{chatId}/subjects/{id:guid}")]
    public async Task<IActionResult> UpdateSubject([FromRoute] string chatId, [FromRoute] Guid id, UpdateSubjectRequest request)
    {
        
        var user = await this.GetUserByChatId(chatId);
        var subject = await this.GetSubject(user, id);

        subject.Name = request.Name;
        
        await _dbContext.SaveChangesAsync();

        return Ok(subject);
    }

    [HttpDelete]
    [Route("api/{chatId}/subjects/{id:guid}")]
    public async Task<IActionResult> DeleteSubject([FromRoute] string chatId, [FromRoute] Guid id)
    {
        
        var user = await this.GetUserByChatId(chatId);
        var subject = await this.GetSubject(user, id);


        _dbContext.Remove(subject);
        await _dbContext.SaveChangesAsync();

        return Ok();
    }
    
    // Events
    [HttpGet]
    [Route("api/{chatId}/events")]
    public async Task<IActionResult> GetEventEvents([FromRoute] string chatId)
    {
        
        var user = await this.GetUserByChatId(chatId);
        
        var events = await _dbContext.Events.Where(s => s.UserId == user.Id).ToListAsync();
        return Ok(
            events
        );
    }
    
    [HttpPost]
    [Route("api/{chatId}/events")]
    public async Task<IActionResult> CreateEvent([FromRoute] string chatId, CreateEventRequest request)
    {
        
        var user = await this.GetUserByChatId(chatId);
        var subject = await this.GetSubject(user, request.SubjectId);

        var _event = new Event
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            SubjectId = subject.Id,
            Name = request.Name,
            Mark = request.Mark
        };
        
        _dbContext.Events.Add(_event);
        await _dbContext.SaveChangesAsync();

        return Ok(_event);
    }

    [HttpDelete]
    [Route("api/{chatId}/events/{id:guid}")]
    public async Task<IActionResult> DeleteEvent([FromRoute] string chatId, [FromRoute] Guid id)
    {
        var user = await this.GetUserByChatId(chatId);
        var _event = await this.GetEvent(user, id);


        _dbContext.Remove(_event);
        await _dbContext.SaveChangesAsync();

        return Ok();
    }

    // Services
    
    private async Task<User> GetUserByChatId(string chatId)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.ChatId == chatId);
        if (user == null)
        {
            BadRequest("User not found");
        }
        
        return user;
    }
    
    private async Task<Subject> GetSubject(User user, Guid Id)
    {
        var subject = await _dbContext
            .Subjects
            .Where(s => s.UserId == user.Id)
            .FirstOrDefaultAsync(s => s.Id == Id);
        
        if (subject == null)
        {
            BadRequest("Subject not found");
        }
        
        return subject;
    }
    
    private async Task<Event> GetEvent(User user, Guid Id)
    {
        var _event = await _dbContext
            .Events
            .Where(e => e.UserId == user.Id)
            .FirstOrDefaultAsync(e => e.Id == Id);
        
        if (_event == null)
        {
            BadRequest("Event not found");
        }
        
        return _event;
    }
}