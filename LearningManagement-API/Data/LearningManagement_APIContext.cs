using System;
using Microsoft.EntityFrameworkCore;
using LearningManagement_API.Model;

namespace LearningManagement_API.Data
{
    public class LearningManagement_APIContext : DbContext
    {
        public LearningManagement_APIContext(DbContextOptions<LearningManagement_APIContext> options)
            : base(options)
        {
        }

        public DbSet<Quiz> Quizzes { get; set; } = default!;
        public DbSet<Question> Questions { get; set; } = default!;
        public DbSet<AnswerOption> AnswerOptions { get; set; } = default!;
        public DbSet<User> Users { get; set; } = default!;
        public DbSet<QuizAttempt> QuizAttempts { get; set; } = default!;
        public DbSet<QuizAttemptAnswer> QuizAttemptAnswers { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<QuizAttemptAnswer>()
                .HasOne(qaa => qaa.Question)
                .WithMany()
                .HasForeignKey(qaa => qaa.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<QuizAttemptAnswer>()
                .HasOne(qaa => qaa.QuizAttempt)
                .WithMany(qa => qa.Answers)
                .HasForeignKey(qaa => qaa.QuizAttemptId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<QuizAttemptAnswer>()
                .HasOne(qaa => qaa.SelectedAnswerOption)
                .WithMany()
                .HasForeignKey(qaa => qaa.SelectedAnswerOptionId)
                .OnDelete(DeleteBehavior.NoAction);
        }

    }
}
