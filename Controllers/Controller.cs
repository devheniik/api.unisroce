using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tgbotapi.Data;
using tgbotapi.Models;
using tgbotapi.Requests;
using Newtonsoft.Json;

namespace tgbotapi.Controllers;

[ApiController]
[Route("[controller]")]
public class Controller : Microsoft.AspNetCore.Mvc.Controller
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
    
    [HttpGet]
    [Route("api/{chatId}/users")]
    public async Task<IActionResult> GetUser([FromRoute] string chatId)
    {
        var user = await this.GetUserByChatId(chatId);

        return Ok(user);
    }
    
    [HttpGet]
    [Route("api/{chatId}/users/check")]
    public async Task<IActionResult> CheckUserExist([FromRoute] string chatId)
    {
        var user_count = _dbContext.Users.Where(u => u.ChatId == chatId).Count();

        if (user_count > 0)
        {
            return Ok(true);
        }

        return Ok(false);
    }
    
    [HttpPut]
    [Route("api/{chatId}/users/update")]
    public async Task<IActionResult> UpdateUser([FromRoute] string chatId, UpdateUserMetaData request)
    {
        var user = await this.GetUserByChatId(chatId);

        user.TempEventName = request.TempEventName;
        user.SubjectId = request.SubjectId;
        user.GroupId = request.GroupId;
        user.CurrentAction = request.CurrentAction;
        
        await _dbContext.SaveChangesAsync();

        return Ok(user);
    }

    // Api

    [HttpGet]
    [Route("api/groups")]
    public async Task<IActionResult> GetGroups()
    {
        HttpClient client = new HttpClient();
        var response = await client.GetAsync("https://schedule.kpi.ua/api/schedule/groups");
        response.EnsureSuccessStatusCode();
        string responseBody = await response.Content.ReadAsStringAsync();

        GroupResponse groupResponse = JsonConvert.DeserializeObject<GroupResponse>(responseBody);

        Item[] groups = groupResponse.data.Where(item => item.faculty == "ФІОТ").ToArray();

        return Ok(groups);
    }
    
    
    // Api

    [HttpGet]
    [Route("api/{chatId}/subjects/import/show")]
    public async Task<IActionResult> GetImportedSubjects([FromRoute] string chatId)
    {
        var user = await this.GetUserByChatId(chatId); 
        
        return Ok(await this.GetSubjectNames(user));
    }
    
    
    [HttpPost]
    [Route("api/{chatId}/subjects/import/apply")]
    public async Task<IActionResult> ApplyImportedSubjects([FromRoute] string chatId)
    {
        var user = await this.GetUserByChatId(chatId); 
        var names = await this.GetSubjectNames(user);
        
        foreach (var name in names)
        {
            var subject = new Subject
            {
                Id = Guid.NewGuid(),
                Name = name,
                UserId = user.Id
            };
            
            _dbContext.Subjects.Add(subject);
        }
        
        await _dbContext.SaveChangesAsync();
        
        return Ok(1);
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
    public async Task<IActionResult> UpdateSubject([FromRoute] string chatId, [FromRoute] Guid id,
        UpdateSubjectRequest request)
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

        var events = await _dbContext.Events.Where(e => e.SubjectId == subject.Id).ToListAsync();
        
        foreach (var _event in events)
        {
            _dbContext.Events.Remove(_event);
        }

        _dbContext.Remove(subject);
        await _dbContext.SaveChangesAsync();

        return Ok(true);
    }

// Events
    [HttpGet]
    [Route("api/{chatId}/events")]
    public async Task<IActionResult> GetEvents([FromRoute] string chatId)
    {
        var user = await this.GetUserByChatId(chatId);

        var events = await _dbContext.Events.Where(s => s.UserId == user.Id).ToListAsync();
        
        var response = new List<TransformerEvent>();

        foreach (var _event in events)
        {
            var tempEvent = new TransformerEvent()
            {
                Id = _event.Id,
                Name = _event.Name,
                Mark = _event.Mark,
                SubjectId = _event.SubjectId,
                SubjectName = _dbContext.Subjects.Where(s => s.Id == _event.SubjectId).FirstOrDefault().Name
            };
            
            response.Add(tempEvent);
        }
        
        return Ok(
            response
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

        return Ok(true);
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
    
    
    private async Task<string[]> GetSubjectNames(User user)
    {
        if (user.GroupId == null)
        {
            BadRequest("User without group");
        }
        
        HttpClient client = new HttpClient();
        var response = await client.GetAsync("https://schedule.kpi.ua/api/schedule/lessons?groupId=" + user.GroupId);
        response.EnsureSuccessStatusCode();
        string responseBody = await response.Content.ReadAsStringAsync();

        LessonsResponse lessonsResponse = JsonConvert.DeserializeObject<LessonsResponse>(responseBody);

        var pairs = new List<Pair>();

        foreach (var day in lessonsResponse.data.scheduleFirstWeek)
        {
            foreach (var pair in day.pairs)
            {
                pairs.Add(pair);
            }
        }
        
        foreach (var day in lessonsResponse.data.scheduleSecondWeek)
        {
            foreach (var pair in day.pairs)
            {
                pairs.Add(pair);
            }
        }

        string[] subjectNames = pairs.Select(obj => obj.name).Distinct().ToArray();

        return subjectNames;
    }
}