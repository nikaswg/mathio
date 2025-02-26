using System.Collections.Generic;

namespace DataLayer
{
    public class QuizViewModel
    {
        public int TotalQuestions { get; set; }
        public int Score { get; set; }
        public Dictionary<string, string> UserAnswers { get; set; }
    }
}