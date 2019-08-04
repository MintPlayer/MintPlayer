using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MintPlayer.Data.Repositories.Interfaces;
using MintPlayer.Web.ViewModels.Subject;

namespace MintPlayer.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubjectController : Controller
    {
        private ISubjectRepository subjectRepository;
        public SubjectController(ISubjectRepository subjectRepository)
        {
            this.subjectRepository = subjectRepository;
        }

        [HttpGet("{subject_id}/likes")]
        public async Task<SubjectLikeVM> Likes([FromRoute]int subject_id)
        {
            var likes = await subjectRepository.GetLikes(subject_id);
            bool authenticated; bool? doeslike;
            try
            {
                doeslike = await subjectRepository.DoesLike(subject_id);
                authenticated = true;
            }
            catch (UnauthorizedAccessException)
            {
                doeslike = null;
                authenticated = false;
            }

            return new SubjectLikeVM
            {
                Likes = likes.Item1,
                Dislikes = likes.Item2,
                Like = doeslike,
                Authenticated = authenticated
            };
        }

        [Authorize]
        [HttpPost("{subject_id}/likes")]
        public async Task<SubjectLikeVM> Like([FromRoute]int subject_id, [FromBody]bool like)
        {
            await subjectRepository.Like(subject_id, like);
            await subjectRepository.SaveChangesAsync();
            var likes = await subjectRepository.GetLikes(subject_id);
            var doeslike = await subjectRepository.DoesLike(subject_id);
            return new SubjectLikeVM
            {
                Likes = likes.Item1,
                Dislikes = likes.Item2,
                Like = doeslike,
                Authenticated = true
            };
        }
    }
}