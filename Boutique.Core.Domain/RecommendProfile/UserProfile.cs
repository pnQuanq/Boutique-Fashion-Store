namespace Boutique.Core.Domain.RecommendProfile
{
    public class UserProfile
    {
        public string MostFrequentCategory { get; set; }
        public double AveragePrice { get; set; }
        public int PreferredGender { get; set; }
        public List<string> Features => new List<string>
                                                    {
                                                        MostFrequentCategory,
                                                        AveragePrice.ToString(),
                                                        PreferredGender.ToString()
                                                    };
    }
}
