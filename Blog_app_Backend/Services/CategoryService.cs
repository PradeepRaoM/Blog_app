using Blog_app_backend.Models;
using Blog_app_backend.Supabase;
using Supabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blog_app_backend.Services
{
    public class CategoryService
    {
        private readonly Client _client;

        public CategoryService(SupabaseClientProvider provider)
        {
            _client = provider.GetClient();
        }

        public async Task<CategoryDto> CreateCategoryAsync(Category category)
        {
            var result = await _client.From<Category>().Insert(category);
            var created = result.Models.FirstOrDefault();

            return created == null ? null : new CategoryDto
            {
                Id = created.Id,
                Name = created.Name,
                Description = created.Description
            };
        }

        public async Task<List<CategoryDto>> GetAllCategoriesAsync()
        {
            var result = await _client.From<Category>().Get();

            return result.Models.Select(cat => new CategoryDto
            {
                Id = cat.Id,
                Name = cat.Name,
                Description = cat.Description
            }).ToList();
        }

        public async Task<CategoryDto> GetCategoryByIdAsync(Guid id)
        {
            var result = await _client.From<Category>().Where(c => c.Id == id).Get();
            var cat = result.Models.FirstOrDefault();

            return cat == null ? null : new CategoryDto
            {
                Id = cat.Id,
                Name = cat.Name,
                Description = cat.Description
            };
        }

        public async Task<bool> DeleteCategoryAsync(Guid id)    
        {
            var existing = await _client.From<Category>().Where(c => c.Id == id).Get();
            if (existing.Models.Count == 0)
                return false;

            await _client.From<Category>().Where(c => c.Id == id).Delete();
            return true;
        }

    }
}
