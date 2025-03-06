using System;

namespace DataLayer
{
    public class TestAttempt
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public DateTime AttemptDate { get; set; }
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public string UserAnswers { get; set; }
        public string name_test { get; set; }
    }
}