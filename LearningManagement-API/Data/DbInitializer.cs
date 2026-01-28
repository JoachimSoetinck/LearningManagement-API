using LearningManagement_API.Data;
using LearningManagement_API.Model;
using Microsoft.EntityFrameworkCore;

public static class DbInitializer
{
    public static void Seed(LearningManagement_APIContext context)
    {
       context.Database.Migrate();

        if (!context.Quizzes.Any())
        {
            var quiz = new Quiz
            {
                Title = "Security Awareness",
                TimeLimitInMinutes = 30,
                MaxAttemptsPerUser = 3,
                PassingScorePercentage = 80,
                IsPublished = true,
                Questions = new List<Question>
                {
                    new Question
                    {
                        Text = "What is the strongest password?",
                        AnswerOptions = new List<AnswerOption>
                        {
                            new AnswerOption { Text = "Password123", IsCorrect = false },
                            new AnswerOption { Text = "A long password with symbols and numbers", IsCorrect = true }
                        }
                    },
                    new Question
                    {
                        Text = "What should you do when you receive a phishing email?",
                        AnswerOptions = new List<AnswerOption>
                        {
                            new AnswerOption { Text = "Ignore the email", IsCorrect = false },
                            new AnswerOption { Text = "Report it to IT security", IsCorrect = true }
                        }
                    }
                }
            };

            context.Quizzes.Add(quiz);
        }

        if (!context.Users.Any())
        {
            context.Users.AddRange(
                new User
                {
                    Email = "admin@test.com",
                    FullName = "Admin User",
                    PasswordHash = "$2a$11$mrqGfIkqyS5upfGSxwr2hONR3UTiXffjG7QxQEY8tEwVvdS0oSxyG",
                    Role = "Admin"
                },
                new User
                {
                    Email = "employee1@company.com",
                    FullName = "Employee One",
                    PasswordHash = "$2a$11$mrqGfIkqyS5upfGSxwr2hONR3UTiXffjG7QxQEY8tEwVvdS0oSxyG",
                    Role = "User"
                }
            );
        }

        context.SaveChanges();
    }
}
