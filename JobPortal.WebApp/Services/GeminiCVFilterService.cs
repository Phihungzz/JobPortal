using JobPortal.WebApp.Models;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace JobPortal.WebApp.Services
{
    public class GeminiCVFilterService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public GeminiCVFilterService(HttpClient httpClient, IOptions<GeminiSettings> settings)
        {
            _httpClient = httpClient;
            _apiKey = settings.Value.ApiKey;
        }

        public async Task<(string Skills, float ExperienceYears, float MatchScore)> AnalyzeCVAsync(string cvContent, string jobDescription, string jobExperience)
        {
            if (string.IsNullOrEmpty(cvContent) || string.IsNullOrEmpty(jobDescription) || string.IsNullOrEmpty(jobExperience))
                throw new ArgumentException("CV content, Job description, and Job experience cannot be empty.");

            
            var prompt = $@"You are an HR Manager with 20 years of experience. Your task is to analyze the CV below and extract 
                            key skills and years of experience. Then compare the CV with the job description 
                            and required experience to calculate a match score.

**Instructions:**
- Extract skills from the CV by looking for keywords like 'Skill', 'Skills', or technical terms (e.g., C#, Java, Python). Skills can appear anywhere in the CV.
- Extract years of experience from the CV by looking for phrases like 'X years experience', 'X years', or time periods related to work. If not found, return 0.
- Extract required skills by analyzing both the Job Description and Required Experience sections:
  - **Primary Skills**: Identify the primary technical skills explicitly listed in the Job Description (e.g., JavaScript, ReactJS from 'Senior Frontend Engineer (JavaScript/ReactJS)'). Treat each term as a separate skill (e.g., 'JavaScript/ReactJS' means two skills: JavaScript and ReactJS).
  - **Additional Skills from Job Experience**: Only include skills from the Required Experience section if they are explicitly marked as required (e.g., phrases like 'must have', 'required', or directly tied to years of experience, such as '8 years experience in JavaScript'). Do NOT include secondary tools or frameworks (e.g., Angular, RXJS, CI/CD tools like Jenkins or Kubernetes) unless they are explicitly listed as primary requirements in either section.
- Calculate two percentages:
  - **JobDescriptionMatch%**: Follow these steps:
    1. Identify the total number of required skills from the Job Description and Required Experience (e.g., JavaScript, ReactJS = 2 skills).
    2. Count the number of matching skills between the CV and the required skills.
    3. Calculate the percentage as (number of matching skills) / (total number of required skills) * 100.
    4. Each skill has equal weight. Do NOT consider relevance or importance unless specified.
    5. Example: If the job requires JavaScript and ReactJS (2 skills), and the CV has only JavaScript (1 match), the JobDescriptionMatch is (1/2) * 100 = 50%.
  - **JobExperienceMatch%**: Percentage (0-100) of how well the CV's experience meets the required experience. If CV experience meets or exceeds the requirement, score 100; if it's at least half of the requirement, score 50; otherwise, score 0.
- Calculate **MatchScore** as the average of JobDescriptionMatch% and JobExperienceMatch%: MatchScore = (JobDescriptionMatch% + JobExperienceMatch%) / 2.
- If no skills or experience are found, return empty skills, 0 for experience, and 0 for both percentages and MatchScore.
- Respond strictly in this format (each field on a new line):
  Skills: <list of skills, comma-separated, e.g., C#, Java>
  ExperienceYears: <number, e.g., 3>
  RequiredSkillsList: <list of required skills, comma-separated, e.g., JavaScript, ReactJS>
  TotalRequiredSkills: <number, e.g., 2>
  MatchingSkills: <number, e.g., 1>
  JobDescriptionMatch: <percentage, e.g., 50>
  JobExperienceMatch: <percentage, e.g., 80>
  MatchScore: <average percentage, e.g., 65>

**CV:**
{cvContent}

**Job Description:**
{jobDescription}

**Required Experience:**
{jobExperience}";

            var requestBody = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = prompt } } }
                }
            };

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("X-Goog-Api-Key", _apiKey);
            var requestUri = "https://generativelanguage.googleapis.com/v1/models/gemini-1.5-flash-001:generateContent";

            var response = await _httpClient.PostAsJsonAsync(requestUri, requestBody);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Gemini API Error Response: {errorContent}");
                throw new Exception($"Failed to call Gemini API. Status code: {response.StatusCode}");
            }

            try
            {
                var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>();
                var generatedText = jsonResponse.GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                System.Diagnostics.Debug.WriteLine($"Gemini API Raw Response: {generatedText}");

                if (string.IsNullOrEmpty(generatedText))
                {
                    System.Diagnostics.Debug.WriteLine("Error: Gemini API returned empty response.");
                    throw new Exception("Received empty or invalid response from Gemini API.");
                }

                var skills = string.Empty;
                float experienceYears = 0;
                string requiredSkillsList = string.Empty;
                int totalRequiredSkills = 0;
                int matchingSkills = 0;
                float jobDescriptionMatch = 0;
                float jobExperienceMatch = 0;
                float matchScore = 0;

                foreach (var line in generatedText.Split('\n', StringSplitOptions.RemoveEmptyEntries))
                {
                    var trimmed = line.Trim();
                    if (Regex.IsMatch(trimmed, @"^Skills\s*:", RegexOptions.IgnoreCase))
                    {
                        skills = Regex.Match(trimmed, @"^Skills\s*:(.+)", RegexOptions.IgnoreCase).Groups[1].Value.Trim();
                        System.Diagnostics.Debug.WriteLine($"Parsed Skills: {skills}");
                    }
                    else if (Regex.IsMatch(trimmed, @"^ExperienceYears\s*:", RegexOptions.IgnoreCase))
                    {
                        var value = Regex.Match(trimmed, @"^ExperienceYears\s*:(.+)", RegexOptions.IgnoreCase).Groups[1].Value.Trim();
                        var cleanValue = Regex.Replace(value, @"[^\d.]", "");
                        if (float.TryParse(cleanValue, out float parsedValue))
                        {
                            experienceYears = parsedValue;
                        }
                        System.Diagnostics.Debug.WriteLine($"Parsed ExperienceYears: {experienceYears}");
                    }
                    else if (Regex.IsMatch(trimmed, @"^RequiredSkillsList\s*:", RegexOptions.IgnoreCase))
                    {
                        requiredSkillsList = Regex.Match(trimmed, @"^RequiredSkillsList\s*:(.+)", RegexOptions.IgnoreCase).Groups[1].Value.Trim();
                        System.Diagnostics.Debug.WriteLine($"Parsed RequiredSkillsList: {requiredSkillsList}");
                    }
                    else if (Regex.IsMatch(trimmed, @"^TotalRequiredSkills\s*:", RegexOptions.IgnoreCase))
                    {
                        var value = Regex.Match(trimmed, @"^TotalRequiredSkills\s*:(.+)", RegexOptions.IgnoreCase).Groups[1].Value.Trim();
                        var cleanValue = Regex.Replace(value, @"[^\d.]", "");
                        if (int.TryParse(cleanValue, out int parsedValue))
                        {
                            totalRequiredSkills = parsedValue;
                        }
                        System.Diagnostics.Debug.WriteLine($"Parsed TotalRequiredSkills: {totalRequiredSkills}");
                    }
                    else if (Regex.IsMatch(trimmed, @"^MatchingSkills\s*:", RegexOptions.IgnoreCase))
                    {
                        var value = Regex.Match(trimmed, @"^MatchingSkills\s*:(.+)", RegexOptions.IgnoreCase).Groups[1].Value.Trim();
                        var cleanValue = Regex.Replace(value, @"[^\d.]", "");
                        if (int.TryParse(cleanValue, out int parsedValue))
                        {
                            matchingSkills = parsedValue;
                        }
                        System.Diagnostics.Debug.WriteLine($"Parsed MatchingSkills: {matchingSkills}");
                    }
                    else if (Regex.IsMatch(trimmed, @"^JobDescriptionMatch\s*:", RegexOptions.IgnoreCase))
                    {
                        var value = Regex.Match(trimmed, @"^JobDescriptionMatch\s*:(.+)", RegexOptions.IgnoreCase).Groups[1].Value.Trim();
                        var cleanValue = Regex.Replace(value, @"[^\d.]", "");
                        if (float.TryParse(cleanValue, out float parsedValue))
                        {
                            jobDescriptionMatch = parsedValue;
                        }
                        System.Diagnostics.Debug.WriteLine($"Parsed JobDescriptionMatch: {jobDescriptionMatch}");
                    }
                    else if (Regex.IsMatch(trimmed, @"^JobExperienceMatch\s*:", RegexOptions.IgnoreCase))
                    {
                        var value = Regex.Match(trimmed, @"^JobExperienceMatch\s*:(.+)", RegexOptions.IgnoreCase).Groups[1].Value.Trim();
                        var cleanValue = Regex.Replace(value, @"[^\d.]", "");
                        if (float.TryParse(cleanValue, out float parsedValue))
                        {
                            jobExperienceMatch = parsedValue;
                        }
                        System.Diagnostics.Debug.WriteLine($"Parsed JobExperienceMatch: {jobExperienceMatch}");
                    }
                    else if (Regex.IsMatch(trimmed, @"^MatchScore\s*:", RegexOptions.IgnoreCase))
                    {
                        var value = Regex.Match(trimmed, @"^MatchScore\s*:(.+)", RegexOptions.IgnoreCase).Groups[1].Value.Trim();
                        var cleanValue = Regex.Replace(value, @"[^\d.]", "");
                        if (float.TryParse(cleanValue, out float parsedValue))
                        {
                            matchScore = parsedValue;
                        }
                        System.Diagnostics.Debug.WriteLine($"Parsed MatchScore: {matchScore}");
                    }
                }

                if (string.IsNullOrEmpty(skills) && experienceYears == 0 && matchScore == 0)
                {
                    System.Diagnostics.Debug.WriteLine("Warning: Could not parse any values from Gemini API response. Returning default values.");
                }

                return (skills, experienceYears, matchScore);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error processing Gemini API response: {ex.Message}");
                throw new Exception("An error occurred while processing the response from Gemini API.", ex);
            }
        }
    }
}