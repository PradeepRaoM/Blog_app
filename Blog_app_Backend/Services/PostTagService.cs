using Blog_app_backend.Models;
using Blog_app_backend.Supabase;
using Supabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blog_app_backend.Services
{
    public class PostTagService
    {
        private readonly Client _supabase;

        public PostTagService(SupabaseClientProvider clientProvider)
        {
            _supabase = clientProvider.GetClient(); // Get Supabase client instance
        }

        // Assign multiple tags to a post
        public async Task AssignTagsToPost(Guid postId, List<Guid> tagIds)
        {
            // Delete existing tags for the post
            await _supabase.From<PostTag>()
                           .Where(pt => pt.PostId == postId)
                           .Delete();

            // Insert new tags
            var postTags = tagIds.Select(id => new PostTag
            {
                PostId = postId,
                TagId = id
            }).ToList();

            if (postTags.Any())
                await _supabase.From<PostTag>().Insert(postTags);
        }

        // Get all tags associated with a post
        public async Task<List<Tag>> GetTagsByPostId(Guid postId)
        {
            var postTagsResponse = await _supabase.From<PostTag>()
                                                  .Where(pt => pt.PostId == postId)
                                                  .Get();

            var tagIds = postTagsResponse.Models.Select(pt => pt.TagId).ToList();
            if (!tagIds.Any()) return new List<Tag>();

            var tags = new List<Tag>();
            foreach (var tagId in tagIds)
            {
                var tagResponse = await _supabase.From<Tag>()
                                                 .Where(t => t.Id == tagId)
                                                 .Get();
                tags.AddRange(tagResponse.Models);
            }

            return tags;
        }

        // NEW: Get all posts associated with a tag
        public async Task<List<PostTag>> GetPostsByTagId(Guid tagId)
        {
            var response = await _supabase.From<PostTag>()
                                          .Where(pt => pt.TagId == tagId)
                                          .Get();

            return response.Models.ToList();
        }
    }
}
