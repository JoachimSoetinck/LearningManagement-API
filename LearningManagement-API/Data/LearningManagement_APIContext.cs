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

            modelBuilder.Entity<Quiz>().HasData(
                new Quiz
                {
                    Id = 1,
                    Title = "Security Awareness",
                    TimeLimitInMinutes = 30,
                    MaxAttemptsPerUser = 3,
                    PassingScorePercentage = 80,
                    IsPublished = true
                }
            );

            modelBuilder.Entity<Question>().HasData(
                new Question { Id = 1, QuizId = 1, Text = "What is the strongest password?" },
                new Question { Id = 2, QuizId = 1, Text = "What should you do when you receive a phishing email?" }
            );

            modelBuilder.Entity<AnswerOption>().HasData(
                new AnswerOption { Id = 1, QuestionId = 1, Text = "Password123", IsCorrect = false },
                new AnswerOption { Id = 2, QuestionId = 1, Text = "A long password with symbols and numbers", IsCorrect = true },
                new AnswerOption { Id = 3, QuestionId = 2, Text = "Ignore the email", IsCorrect = false },
                new AnswerOption { Id = 4, QuestionId = 2, Text = "Report it to IT security", IsCorrect = true }
            );

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Email = "admin@test.com",
                    FullName = "Admin User",
                    PasswordHash = "$2a$11$mrqGfIkqyS5upfGSxwr2hONR3UTiXffjG7QxQEY8tEwVvdS0oSxyG",
                    Role = "Admin"
                },
                new User
                {
                    Id = 2,
                    Email = "employee1@company.com",
                    FullName = "Employee One",
                    PasswordHash = "$2a$11$mrqGfIkqyS5upfGSxwr2hONR3UTiXffjG7QxQEY8tEwVvdS0oSxyG",
                    Role = "User"
                }
            );

            modelBuilder.Entity<QuizAttempt>().HasData(
                new QuizAttempt
                {
                    Id = 1,
                    QuizId = 1,
                    UserId = 1,
                    StartedAt = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc),
                    CompletedAt = new DateTime(2024, 1, 1, 10, 10, 0, DateTimeKind.Utc),
                    ScorePercentage = 100,
                    IsPassed = true
                }
            );

            modelBuilder.Entity<QuizAttemptAnswer>().HasData(
                new QuizAttemptAnswer { Id = 1, QuizAttemptId = 1, QuestionId = 1, SelectedAnswerOptionId = 2 },
                new QuizAttemptAnswer { Id = 2, QuizAttemptId = 1, QuestionId = 2, SelectedAnswerOptionId = 4 }
            );
        }
    }
}
