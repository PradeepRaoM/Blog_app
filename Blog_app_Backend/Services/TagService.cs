using Blog_app_backend.Models;
using Blog_app_backend.Supabase;
using Supabase;
using Supabase.Postgrest.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Supabase.Postgrest.Constants;

namespace Blog_app_backend.Services
{
    public class TagService
    {
        private readonly Client _supabase;

        public TagService(SupabaseClientProvider clientProvider)
        {
            _supabase = clientProvider.GetClient(); // Fixed this line
        }

        public async Task<Tag> CreateTagAsync(Tag tag)
        {
            var response = await _supabase.From<Tag>().Insert(tag);
            return response.Models.First();
        }

        public async Task<List<Tag>> GetAllTagsAsync()
        {
            var result = await _supabase.From<Tag>().Get();
            return result.Models;
        }

        public async Task<Tag?> GetTagByIdAsync(Guid id)
        {
            var result = await _supabase
                .From<Tag>()
                .Where(t => t.Id == id)
                .Get();

            return result.Models.FirstOrDefault();
        }

        public async Task<bool> DeleteTagAsync(Guid id)
        {
            try
            {
                var tag = new Tag { Id = id };
                var response = await _supabase.From<Tag>().Delete(tag);
                return response.Models.Any();
            }
            catch (PostgrestException)
            {
                return false;
            }
        }
        public async Task<Tag?> GetTagByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            var result = await _supabase.From<Tag>()
                                        .Filter("name", Operator.ILike, name)
                                        .Get();

            return result.Models.FirstOrDefault();
        }


    }
}
