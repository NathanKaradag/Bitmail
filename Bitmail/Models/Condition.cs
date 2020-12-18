using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Bitmail.Models
{
	public class Condition
	{
		[JsonPropertyName("condition_type")]
		public string ConditionType { get; set; }

		[JsonPropertyName("field")]
		public string Field { get; set; }

		[JsonPropertyName("op")]
		public string Operator { get; set; }

		[JsonPropertyName("value")]
		public string Value { get; set; }
	}
}