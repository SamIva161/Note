using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Notes.Domain.Enums;
using Notes.Domain.Models;
using Notes.DTOs.Notes.AddNote;
using Notes.DTOs.Notes.DeleteNote;
using Notes.DTOs.Notes.GetListNote;
using Notes.DTOs.Notes.GetNote;
using Notes.DTOs.Notes.UpdateNote;
using Notes.Infrastructure.ApplicationContext;
using Notes.Infrastructure.Security;
using Notes.Infrastucture.Interfaces;
using Notes.Infrastucture.Security;
using Notes.Interfaces;
using System.Linq.Expressions;
using System.Security.Claims;

namespace Notes.Services
{
    public class NotesService : BaseService, INoteService
    {
        private readonly User? user;

        public NotesService(IAsyncRepository<Note> repository, EFContext context, IHttpContextAccessor httpContext)
            : base(repository, context)
        {
            this.user = CurrentUser.GetUser(context, httpContext.HttpContext!.User);
        }

        public async Task<AddNoteResponse> AddNoteAsync(AddNoteRequest request)
        {
            //User? user = await CurrentUser.GetUserAsync(context, request.User);

            if (user != null)
            {
                Note note = new Note()
                {
                    Title = request.Title,
                    Description = request.Description,
                    IsDone = request.IsDone,
                    CreateDate = DateTime.Now,
                    User = user
                };

                await repository.AddAsync(note);

                return new AddNoteResponse()
                {
                    Note = note,
                };
            }
            else
            {
                return new AddNoteResponse()
                {
                    Note = null,
                };
            }
        }

        public async Task<DeleteNoteResponse> DeleteNoteAsync(DeleteNoteRequest request)
        {
            //User? user = await CurrentUser.GetUserAsync(context, request.User);

            Note? note = await repository.GetAsync(note => note.Id == request.Id && user!.Id == note.UserId);

            if(note != null)
            {
                await repository.DeleteAsync(note);
                return new DeleteNoteResponse(true);
            }

            return new DeleteNoteResponse(false);
        }

        public async Task<GetListNoteResponse> GetListNoteAsync(GetListNoteRequest request)
        {
            //User? user = await CurrentUser.GetUserAsync(context, request.User);

            IEnumerable<Note>? notes = await repository.GetAllAsync(note => user!.Id == note.UserId);

            if (notes != null)
            {
                switch (request.Sort)
                {
                    case "asc_date":
                        notes = notes?.OrderBy(note => note.CreateDate);
                        break;
                    case "desc_date":
                        notes = notes?.OrderByDescending(note => note.CreateDate);
                        break;
                };

                var totalNotes = notes?.ToList().Count ?? 0;

                var result = notes?
                    .Skip(request.PageNumber * request.PageSize)
                    .Take(request.PageSize);

                var tatalPages = (int)Math.Ceiling(((decimal)totalNotes / request.PageSize));

                return new GetListNoteResponse()
                {
                    Notes = result,
                    PageSize = request.PageSize,
                    PageNumber = request.PageNumber,
                    TotalNotes = totalNotes,
                    TotalPages = tatalPages,
                };
            }
            else
            {
                return new GetListNoteResponse()
                {
                    Notes = null,
                };
            }

        }

        public async Task<GetNoteResponse> GetNoteAsync(GetNoteRequest request)
        {
            //User? user = await CurrentUser.GetUserAsync(context, request.User);

            Note? note = await repository.GetAsync(note => note.Id == request.Id && user!.Id == note.UserId);

            return new GetNoteResponse()
            {
                Note = note
            };
        }

        public async Task<UpdateNoteResponse> UpdateNoteAsync(UpdateNoteRequest request)
        {
            //User? user = await CurrentUser.GetUserAsync(context, request.User);

            Note? note = await repository.GetAsync(note => note.Id == request.Id && user!.Id == note.UserId);

            if(note != null)
            {
                note.Title = request.Title;
                note.Description = request.Description;
                note.IsDone = request.IsDone;

                await repository.UpdateAsync(note);

                return new UpdateNoteResponse(true);
            }
            return new UpdateNoteResponse(false);
        }
    }
}
