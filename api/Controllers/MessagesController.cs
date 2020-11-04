using System.Collections.Generic;
using System.Threading.Tasks;
using api.DTOs;
using api.Entities;
using api.Extensions;
using api.Helpers;
using api.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
  [Authorize]
  public class MessagesController : BaseApiController
  {
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public MessagesController(IUnitOfWork unitOfWork, IMapper mapper)
    {
      _unitOfWork = unitOfWork;
      _mapper = mapper;
    }

    [HttpPost]
    public async Task<ActionResult<MessageDTO>> CreateMessage(CreateMessageDTO createMessageDTO)
    {
      var username = User.GetUsername();

      if (username == createMessageDTO.RecipientUsername.ToLower()) return BadRequest("You cannot message yourself");

      var sender = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);
      var recipient = await _unitOfWork.UserRepository.GetUserByUsernameAsync(createMessageDTO.RecipientUsername.ToLower());

      if (recipient == null) return NotFound();

      var message = new Message
      {
        SenderId = sender.Id,
        SenderUsername = sender.UserName,
        RecipientId = recipient.Id,
        RecipientUsername = recipient.UserName,
        Content = createMessageDTO.Content
      };

      _unitOfWork.MessagesRepository.AddMessage(message);

      if (await _unitOfWork.Complete()) return Ok(_mapper.Map<MessageDTO>(message));

      return BadRequest("Failed to save message");
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MessageDTO>>> GetMessagesForUser([FromQuery] MessagesParams messagesParams)
    {
      messagesParams.Username = User.GetUsername();

      var messages = await _unitOfWork.MessagesRepository.GetMessagesForUser(messagesParams);

      Response.AddPaginationHeader(messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages);

      return Ok(messages);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMessage(int id)
    {
      var username = User.GetUsername();

      var message = await _unitOfWork.MessagesRepository.GetMessage(id);

      if (message.SenderUsername != username && message.RecipientUsername != username)
        return Unauthorized();

      if (message.SenderUsername == username) message.SenderDeleted = true;

      if (message.RecipientUsername == username) message.RecipientDeleted = true;

      if (message.SenderDeleted && message.RecipientDeleted) _unitOfWork.MessagesRepository.DeleteMessage(message);

      if (await _unitOfWork.Complete()) return Ok();

      return BadRequest("Problem deleting the message");
    }
  }
}
