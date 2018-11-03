using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using pelazem.util;


namespace face.lib
{
	public class Emotion
	{
		public double Anger { get; set; }
		public double Contempt { get; set; }
		public double Disgust { get; set; }
		public double Fear { get; set; }
		public double Happiness { get; set; }
		public double Neutral { get; set; }
		public double Sadness { get; set; }
		public double Surprise { get; set; }

		private List<EmotionTuple> _emotions;

		public IEnumerable<EmotionTuple> Emotions
		{
			get
			{
				if (_emotions == null)
					_emotions = GetEmotions();

				return _emotions;
			}
		}

		// Rather than hard-coding property names to get our list of emotions to allow sorting etc., reflect over the primitive properties
		// While reflection is expensive, this happens once per image
		private List<EmotionTuple> GetEmotions()
		{
			return TypeUtil.GetPrimitiveProps(this.GetType())
				.Select(p => new EmotionTuple() { EmotionName = p.Name, EmotionValue = Converter.GetDouble(p.GetValueEx(this)) })
				.ToList()
			;
		}
	}
}
