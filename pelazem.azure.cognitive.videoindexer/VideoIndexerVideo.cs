using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace pelazem.azure.cognitive.videoindexer
{
	public class VideoIndexerVideo
	{
		// Main properties from Get-Video-Index output: https://api-portal.videoindexer.ai/docs/services/operations/operations/Get-Video-Index?
		public string AccountId { get; set; } // Also at Get-Index root
		public string Id { get; set; } // Also at Get-Index root
		public string Name { get; set; } // Also at Get-Index root
		public string Description { get; set; } // Also at Get-Index root
		public string UserName { get; set; } // Also at Get-Index root
		public DateTime? Created { get; set; } // Also at Get-Index root
		public string PrivacyMode { get; set; } // Also at Get-Index root
		public string State { get; set; } // Also at Get-Index root
		public string ReviewState { get; set; } // At Get-Index/videos/video
		public string ModerationState { get; set; } // At Get-Index/videos/video
		public bool IsOwned { get; set; } // Also at Get-Index root
		public bool IsEditable { get; set; } // Also at Get-Index root
		public bool IsBase { get; set; } // Also at Get-Index root
		public int DurationInSeconds { get; set; } // Also at Get-Index root

		// Main properties from List-Videos output: https://api-portal.videoindexer.ai/docs/services/operations/operations/List-videos?
		public string Partition { get; set; } // Also at Get-Index root
		public string ExternalId { get; set; } // At Get-Index/videos/video
		public string Metadata { get; set; } // At Get-Index/videos/video
		public DateTime? LastModified { get; set; } // Not in Get-Index
		public DateTime? LastIndexed { get; set; } // Not in Get-Index
		public string ProcessingProgress { get; set; } // At Get-Index/videos/video
		public string ThumbNailVideoId { get; set; } // At Get-Index/summarizedInsights
		public string ThumbNailId { get; set; } // At Get-Index/videos/video
		public Social Social { get; set; } // Not in Get-Index
		public string[] SearchMatches { get; set; } // Not in Get-Index
		public string IndexingPreset { get; set; } // At Get-Index/videos/video
		public string StreamingPreset { get; set; } // Not in Get-Index
		public string SourceLanguage { get; set; } // At Get-Index/videos/video

		public SummarizedInsights SummarizedInsights { get; set; }

		public string FailureCode { get; set; } // At Get-Index/videos/video
		public string FailureMessage { get; set; } // At Get-Index/videos/video
		public string ExternalUrl { get; set; } // At Get-Index/videos/video
		public bool IsAdult { get; set; } // At Get-Index/videos/video
		public string PublishedUrl { get; set; } // At Get-Index/videos/video
		public string PublishedUrlProxy { get; set; } // At Get-Index/videos/video
		public string ViewToken { get; set; } // At Get-Index/videos/video
		public string Language { get; set; } // At Get-Index/videos/video
		public string LinguisticModelId { get; set; } // At Get-Index/videos/video
		public Statistics Statistics { get; set; }

		public Insights Insights { get; set; }

		public string Captions { get; set; }

		[JsonIgnore]
		internal string RawJsonFromApi { get; set; }
	}

	public class Insights
	{
		public string Version { get; set; }
		public string Duration { get; set; }
		public string SourceLanguage { get; set; }
		public string Language { get; set; }
		public double SourceLanguageConfidence { get; set; }

		public List<Transcript> Transcript { get; set; }
		public List<Ocr> Ocr { get; set; }
		public List<Keyword> Keywords { get; set; }
		public List<Block> Blocks { get; set; }
		public List<Face> Faces { get; set; }
		public List<Label> Labels { get; set; }
		public List<Shot> Shots { get; set; }
		public List<Brand> Brands { get; set; }
		public List<AudioEffect> AudioEffects { get; set; }
		public List<Sentiment> Sentiments { get; set; }
		public List<VisualContentModeration> VisualContentModeration { get; set; }
		public TextualContentModeration TextualContentModeration { get; set; }
		public List<Emotion> Emotions { get; set; }
		public List<Topic> Topics { get; set; }
	}

	public class Social
	{
		public bool LikedByUser { get; set; }
		public int Likes { get; set; }
		public int Views { get; set; }
	}

	public class SummarizedInsights
	{
		public string Name { get; set; }
		public string Id { get; set; }
		public string PrivacyMode { get; set; }
		public Duration Duration { get; set; }
		public string ThumbNailVideoId { get; set; }
		public string ThumbNailId { get; set; }

		public List<Face> Faces { get; set; }
		public List<Keyword> Keywords { get; set; }
		public List<Sentiment> Sentiments { get; set; }
		public List<AudioEffect> AudioEffects { get; set; }
		public List<Label> Labels { get; set; }
		public List<Brand> Brands { get; set; }
		public Statistics Statistics { get; set; }
		public List<Emotion> Emotions { get; set; }
		public List<Topic> Topics { get; set; }
	}

	public class Duration
	{
		public string Time { get; set; }
		public double Seconds { get; set; }
	}

	public class Face
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public double Confidence { get; set; }
		public string Description { get; set; }
		public string ThumbNailId { get; set; }
		public string KnownPersonId { get; set; }
		public string ReferenceId { get; set; }
		public string Title { get; set; }
		public string ImageUrl { get; set; }

		public List<FaceInstance> Instances { get; set; }
	}

	public class Keyword
	{
		public int Id { get; set; }
		public string Text { get; set; }
		public double Confidence { get; set; }
		public string Language { get; set; }

		public List<Instance> Instances { get; set; }
	}

	public class SentimentAppearance
	{
		public string StartTime { get; set; }
		public string EndTime { get; set; }
		public int StartSeconds { get; set; }
		public int EndSeconds { get; set; }
	}

	public class Sentiment
	{
		public int Id { get; set; }
		public double Score { get; set; }
		public double AverageScore { get; set; }
		public string SentimentType { get; set; }

		public List<AdjustedInstance> Instances { get; set; }

		public string SentimentKey { get; set; }
		public int SeenDurationRatio { get; set; }

		public List<SentimentAppearance> Appearances { get; set; }
	}

	public class AudioEffect
	{
		public int Id { get; set; }
		public string Type { get; set; }

		public List<Instance> Instances { get; set; }
	}

	public class Label
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Language { get; set; }

		public List<LabelInstance> Instances { get; set; }
	}

	public class Brand
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string ReferenceId { get; set; }
		public string ReferenceUrl { get; set; }
		public string ReferenceType { get; set; }
		public string Description { get; set; }
		List<string> Tags { get; set; }
		public double Confidence { get; set; }

		public List<BrandInstance> Instances { get; set; }
	}

	public class Statistics
	{
		public int CorrespondenceCount { get; set; }

		// TODO implement the following - docs has no sample JSON etc.
		//public string SpeakerTalkToListenRatio { get; set; }
		//public string SpeakerLongestMonolog { get; set; }
		//public string SpeakerNumberOfFragments { get; set; }
		//public string SpeakerWordCount { get; set; }
	}

	public class Emotion
	{
		public int Id { get; set; }
		public string Type { get; set; }

		public List<EmotionInstance> Instances { get; set; }
	}

	public class Topic
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string ReferenceId { get; set; }
		public string ReferenceType { get; set; }
		public double Confidence { get; set; }
		public string Language { get; set; }

		public List<TopicInstance> Instances { get; set; }
	}

	public class Transcript
	{
		public int Id { get; set; }
		public string Text { get; set; }
		public double Confidence { get; set; }
		public int SpeakerId { get; set; }
		public string Language { get; set; }

		public List<AdjustedInstance> Instances { get; set; }
	}

	public class Ocr
	{
		public int Id { get; set; }
		public string Text { get; set; }
		public double Confidence { get; set; }
		public int Left { get; set; }
		public int Top { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public string Language { get; set; }

		public List<AdjustedInstance> Instances { get; set; }
	}

	public class Shot
	{
		public int Id { get; set; }

		public List<KeyFrame> KeyFrames { get; set; }
		public List<ShotInstance> Instances { get; set; }
	}

	public class KeyFrame
	{
		public int Id { get; set; }

		public List<KeyFrameInstance> Instances { get; set; }
	}

	public class Block
	{
		public int Id { get; set; }

		public List<AdjustedInstance> Instances { get; set; }
	}

	public class Speaker
	{
		public int Id { get; set; }
		public string Name { get; set; }

		public List<AdjustedInstance> Instances { get; set; }
	}

	public class VisualContentModeration
	{
		public int Id { get; set; }
		public double AdultScore { get; set; }
		public double RacyScore { get; set; }

		public List<AdjustedInstance> Instances { get; set; }
	}

	public class TextualContentModeration
	{
		public int Id { get; set; }
		public int BannedWordsCount { get; set; }
		public double BannedWordsRatio { get; set; }

		public List<AdjustedInstance> Instances { get; set; }
	}


	public class Instance
	{
		public virtual string Start { get; set; }
		public virtual string End { get; set; }
	}

	public class AdjustedInstance : Instance
	{
		public virtual string AdjustedStart { get; set; }
		public virtual string AdjustedEnd { get; set; }
	}

	public class FaceInstance : AdjustedInstance
	{
		public virtual List<string> ThumbnailsIds { get; set; }
	}

	public class LabelInstance : Instance
	{
		public virtual double Confidence { get; set; }
	}

	public class BrandInstance : Instance
	{
		public virtual string BrandType { get; set; }
	}

	public class EmotionInstance : AdjustedInstance
	{
	}

	public class TopicInstance : AdjustedInstance
	{
	}

	public class ShotInstance : AdjustedInstance
	{
	}

	public class KeyFrameInstance : AdjustedInstance
	{
		public string ThumbnailId { get; set; }
	}
}
