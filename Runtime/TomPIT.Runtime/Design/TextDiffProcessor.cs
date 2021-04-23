using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace TomPIT.Design
{
	internal static class TextDiffProcessor
	{
		private static readonly Regex BlankLineEnd = new Regex("\\n\\r?\\n\\Z");
		private static readonly Regex BlankLineStart = new Regex("\\A\\r?\\n\\r?\\n");

		private const short EditCost = 4;
		private const double MatchThreshold = 0.5d;
		private const int MatchDistance = 1000;
		private const float PatchDeleteThreshold = 0.5f;
		private const short PatchMargin = 4;
		private const short MatchMaxBits = 32;

		//public float Timeout { get; set; } = 0f;

		public static List<ITextDiffDescriptor> Diff(TextDiffArgs e)
		{
			switch (e.Mode)
			{
				case TextDiffCompareMode.Char:
					return Diff(e, true);
				case TextDiffCompareMode.Word:
				case TextDiffCompareMode.Line:
					var linesResult = LinesToChars(e);
					var diffs = Diff(e.WithNewText(linesResult.Text1, linesResult.Text2), false);

					CharsToLines(diffs, linesResult.Lines);

					return diffs;
				default:
					throw new NotSupportedException();
			}
		}

		private static List<ITextDiffDescriptor> Diff(TextDiffArgs e, bool checklines)
		{
			var deadline = DateTime.MaxValue;

			if (e.Timeout > 0)
				deadline = DateTime.Now.Add(new TimeSpan(((long)(e.Timeout * 1000)) * 10000));

			return Diff(e, checklines, deadline);
		}

		private static List<ITextDiffDescriptor> Diff(TextDiffArgs e, bool checklines, DateTime deadline)
		{
			List<ITextDiffDescriptor> diffs;

			if (string.Compare(e.Original, e.Modified, StringComparison.Ordinal) == 0)
			{
				diffs = new List<ITextDiffDescriptor>();

				if (e.Original.Length != 0)
					diffs.Add(new TextDiffDescriptor(TextDiffOperation.Equal, e.Original));

				return diffs;
			}

			var commonLength = CommonPrefix(e.Original, e.Modified);
			var commonPrefix = e.Original.Substring(0, commonLength);

			e.Original = e.Original[commonLength..];
			e.Modified = e.Modified[commonLength..];

			commonLength = CommonSuffix(e.Original, e.Modified);

			var commonsuffix = e.Original[^commonLength..];

			e.Original = e.Original.Substring(0, e.Original.Length - commonLength);
			e.Modified = e.Modified.Substring(0, e.Modified.Length - commonLength);

			diffs = Compute(e, checklines, deadline);

			if (commonPrefix.Length != 0)
				diffs.Insert(0, (new TextDiffDescriptor(TextDiffOperation.Equal, commonPrefix)));

			if (commonsuffix.Length != 0)
				diffs.Add(new TextDiffDescriptor(TextDiffOperation.Equal, commonsuffix));

			CleanupMerge(diffs);

			return diffs;
		}

		private static List<ITextDiffDescriptor> Compute(TextDiffArgs e, bool checklines, DateTime deadline)
		{
			var diffs = new List<ITextDiffDescriptor>();

			if (string.IsNullOrEmpty(e.Original))
			{
				diffs.Add(new TextDiffDescriptor(TextDiffOperation.Insert, e.Modified));

				return diffs;
			}

			if (string.IsNullOrEmpty(e.Modified))
			{
				diffs.Add(new TextDiffDescriptor(TextDiffOperation.Delete, e.Original));

				return diffs;
			}

			var longText = e.Original.Length > e.Modified.Length ? e.Original : e.Modified;
			var shortText = e.Original.Length > e.Modified.Length ? e.Modified : e.Original;
			var i = longText.IndexOf(shortText, StringComparison.Ordinal);

			if (i != -1)
			{
				var op = (e.Original.Length > e.Modified.Length) ? TextDiffOperation.Delete : TextDiffOperation.Insert;

				diffs.Add(new TextDiffDescriptor(op, longText.Substring(0, i)));
				diffs.Add(new TextDiffDescriptor(TextDiffOperation.Equal, shortText));
				diffs.Add(new TextDiffDescriptor(op, longText[(i + shortText.Length)..]));

				return diffs;
			}

			if (shortText.Length == 1)
			{
				diffs.Add(new TextDiffDescriptor(TextDiffOperation.Delete, e.Original));
				diffs.Add(new TextDiffDescriptor(TextDiffOperation.Insert, e.Modified));

				return diffs;
			}

			var hm = HalfMatch(e);

			if (hm != null)
			{
				var text1A = hm[0];
				var text1B = hm[1];
				var text2A = hm[2];
				var text2B = hm[3];
				var midCommon = hm[4];

				var diffsA = Diff(e.WithNewText(text1A, text2A), checklines, deadline);
				var diffsB = Diff(e.WithNewText(text1B, text2B), checklines, deadline);

				diffs = diffsA;

				diffs.Add(new TextDiffDescriptor(TextDiffOperation.Equal, midCommon));
				diffs.AddRange(diffsB);

				return diffs;
			}

			if (checklines && e.Original.Length > 100 && e.Modified.Length > 100)
				return LineMode(e, deadline);

			return Bisect(e, deadline);
		}

		private static List<ITextDiffDescriptor> LineMode(TextDiffArgs e, DateTime deadline)
		{
			var args = e.Copy();

			args.Mode = TextDiffCompareMode.Line;

			var linesResult = LinesToChars(e);
			var diffs = Diff(e.WithNewText(linesResult.Text1, linesResult.Text2), false, deadline);

			CharsToLines(diffs, linesResult.Lines);
			CleanupSemantic(diffs);

			diffs.Add(new TextDiffDescriptor(TextDiffOperation.Equal, string.Empty));

			var pointer = 0;
			var countDelete = 0;
			var countInsert = 0;
			var textDelete = string.Empty;
			var textInsert = string.Empty;

			while (pointer < diffs.Count)
			{
				switch (diffs[pointer].Operation)
				{
					case TextDiffOperation.Insert:
						countInsert++;
						textInsert += diffs[pointer].Text;
						break;
					case TextDiffOperation.Delete:
						countDelete++;
						textDelete += diffs[pointer].Text;
						break;
					case TextDiffOperation.Equal:
						if (countDelete >= 1 && countInsert >= 1)
						{
							diffs.RemoveRange(pointer - countDelete - countInsert, countDelete + countInsert);
							pointer = pointer - countDelete - countInsert;

							var subDiff = Diff(e.WithNewText(textDelete, textInsert), false, deadline);

							diffs.InsertRange(pointer, subDiff);

							pointer += subDiff.Count;
						}

						countInsert = 0;
						countDelete = 0;

						textDelete = string.Empty;
						textInsert = string.Empty;

						break;
				}

				pointer++;
			}

			diffs.RemoveAt(diffs.Count - 1);

			return diffs;
		}

		private static List<ITextDiffDescriptor> Bisect(TextDiffArgs e, DateTime deadline)
		{
			var text1Length = e.Original.Length;
			var text2Length = e.Modified.Length;
			var maxD = (text1Length + text2Length + 1) / 2;
			var vOffset = maxD;
			var vLength = 2 * maxD;
			var v1 = new int[vLength];
			var v2 = new int[vLength];

			for (var i = 0; i < vLength; i++)
			{
				v1[i] = -1;
				v2[i] = -1;
			}

			v1[vOffset + 1] = 0;
			v2[vOffset + 1] = 0;

			var delta = text1Length - text2Length;
			var front = (delta % 2 != 0);
			var j1Start = 0;
			var j1End = 0;
			var j2Start = 0;
			var j2End = 0;

			for (var i = 0; i < maxD; i++)
			{
				if (DateTime.Now > deadline)
					break;

				for (var j = -i + j1Start; j <= i - j1End; j += 2)
				{
					var j1Offset = vOffset + j;
					int x1;

					if (j == -i || j != i && v1[j1Offset - 1] < v1[j1Offset + 1])
						x1 = v1[j1Offset + 1];
					else
						x1 = v1[j1Offset - 1] + 1;

					var y1 = x1 - j;

					while (x1 < text1Length && y1 < text2Length && e.Original[x1] == e.Modified[y1])
					{
						x1++;
						y1++;
					}

					v1[j1Offset] = x1;

					if (x1 > text1Length)
						j1End += 2;
					else if (y1 > text2Length)
						j1Start += 2;
					else if (front)
					{
						var j2Offset = vOffset + delta - j;

						if (j2Offset >= 0 && j2Offset < vLength && v2[j2Offset] != -1)
						{
							var x2 = text1Length - v2[j2Offset];

							if (x1 >= x2)
								return BisectSplit(e, x1, y1, deadline);
						}
					}
				}

				for (var j2 = -i + j2Start; j2 <= i - j2End; j2 += 2)
				{
					var j2Offset = vOffset + j2;
					int x2;

					if (j2 == -i || j2 != i && v2[j2Offset - 1] < v2[j2Offset + 1])
						x2 = v2[j2Offset + 1];
					else
						x2 = v2[j2Offset - 1] + 1;

					var y2 = x2 - j2;

					while (x2 < text1Length && y2 < text2Length && e.Original[text1Length - x2 - 1] == e.Modified[text2Length - y2 - 1])
					{
						x2++;
						y2++;
					}

					v2[j2Offset] = x2;

					if (x2 > text1Length)
						j2End += 2;
					else if (y2 > text2Length)
						j2Start += 2;
					else if (!front)
					{
						var j1Offset = vOffset + delta - j2;

						if (j1Offset >= 0 && j1Offset < vLength && v1[j1Offset] != -1)
						{
							var x1 = v1[j1Offset];
							var y1 = vOffset + x1 - j1Offset;

							x2 = text1Length - v2[j2Offset];

							if (x1 >= x2)
								return BisectSplit(e, x1, y1, deadline);
						}
					}
				}
			}

			var diffs = new List<ITextDiffDescriptor>
			{
				new TextDiffDescriptor(TextDiffOperation.Delete, e.Original),
				new TextDiffDescriptor(TextDiffOperation.Insert, e.Modified)
			};

			return diffs;
		}

		private static List<ITextDiffDescriptor> BisectSplit(TextDiffArgs e, int x, int y, DateTime deadline)
		{
			var text1a = e.Original.Substring(0, x);
			var text2a = e.Modified.Substring(0, y);
			var text1b = e.Original[x..];
			var text2b = e.Modified[y..];

			var diffs = Diff(e.WithNewText( text1a, text2a), false, deadline);
			var diffsb = Diff(e.WithNewText(text1b, text2b), false, deadline);

			diffs.AddRange(diffsb);

			return diffs;
		}

		private static TextDiffLinesResult LinesToChars(TextDiffArgs e)
		{
			var result = new TextDiffLinesResult();
			var lineHash = new Dictionary<string, int>();

			result.Lines.Add(string.Empty);

			result.Text1 = LinesToCharsMunge(e.Original, result.Lines, lineHash, 40000, e.Mode);
			result.Text2 = LinesToCharsMunge(e.Modified, result.Lines, lineHash, 65535, e.Mode);

			return result;
		}


		private static string LinesToCharsMunge(string text, List<string> lineArray, Dictionary<string, int> lineHash, int maxLines, TextDiffCompareMode mode)
		{
			var lineStart = 0;
			var lineEnd = -1;
			string line;
			var chars = new StringBuilder();

			while (lineEnd < text.Length - 1)
			{
				lineEnd = mode == TextDiffCompareMode.Line
					? text.IndexOf('\n', lineStart)
					: text.IndexOfAny(new char[] { '\n', ' ' }, lineStart);

				if (lineEnd == -1)
					lineEnd = text.Length - 1;

				line = text[lineStart..(lineEnd + 1)];

				if (lineHash.ContainsKey(line))
					chars.Append((char)lineHash[line]);
				else
				{
					if (lineArray.Count == maxLines)
					{
						line = text[lineStart..];
						lineEnd = text.Length;
					}

					lineArray.Add(line);
					lineHash.Add(line, lineArray.Count - 1);
					chars.Append((char)(lineArray.Count - 1));
				}

				lineStart = lineEnd + 1;
			}

			return chars.ToString();
		}

		private static void CharsToLines(ICollection<ITextDiffDescriptor> diffs, IList<string> lineArray)
		{
			foreach (var diff in diffs)
			{
				var text = new StringBuilder();

				for (var i = 0; i < diff.Text.Length; i++)
					text.Append(lineArray[diff.Text[i]]);

				diff.Text = text.ToString();
			}
		}

		private static int CommonPrefix(string text1, string text2)
		{
			var n = Math.Min(text1.Length, text2.Length);

			for (var i = 0; i < n; i++)
			{
				if (text1[i] != text2[i])
					return i;
			}

			return n;
		}

		private static int CommonSuffix(string text1, string text2)
		{
			var text1Length = text1.Length;
			var text2Length = text2.Length;
			var n = Math.Min(text1.Length, text2.Length);

			for (int i = 1; i <= n; i++)
			{
				if (text1[text1Length - i] != text2[text2Length - i])
					return i - 1;
			}

			return n;
		}

		private static int CommonOverlap(string text1, string text2)
		{
			var text1Length = text1.Length;
			var text2Length = text2.Length;

			if (text1Length == 0 || text2Length == 0)
				return 0;

			if (text1Length > text2Length)
				text1 = text1[(text1Length - text2Length)..];
			else if (text1Length < text2Length)
				text2 = text2.Substring(0, text1Length);

			var textLength = Math.Min(text1Length, text2Length);

			if (string.Compare(text1, text2, StringComparison.Ordinal) == 0)
				return textLength;

			var best = 0;
			var length = 1;

			while (true)
			{
				var pattern = text1[(textLength - length)..];
				var found = text2.IndexOf(pattern, StringComparison.Ordinal);

				if (found == -1)
					return best;

				length += found;

				if (found == 0 || string.Compare(text1[(textLength - length)..], text2.Substring(0, length), StringComparison.Ordinal) == 0)
				{
					best = length;
					length++;
				}
			}
		}

		private static string[] HalfMatch(TextDiffArgs e)
		{
			if (e.Timeout <= 0)
				return null;

			var longText = e.Original.Length > e.Modified.Length ? e.Original : e.Modified;
			var shortText = e.Original.Length > e.Modified.Length ? e.Modified : e.Original;

			if (longText.Length < 4 || shortText.Length * 2 < longText.Length)
				return null;

			var hm1 = HalfMatch(longText, shortText, (longText.Length + 3) / 4);
			var hm2 = HalfMatch(longText, shortText, (longText.Length + 1) / 2);
			string[] hm;

			if (hm1 == null && hm2 == null)
				return null;
			else if (hm2 == null)
				hm = hm1;
			else if (hm1 == null)
				hm = hm2;
			else
				hm = hm1[4].Length > hm2[4].Length ? hm1 : hm2;

			if (e.Original.Length > e.Modified.Length)
				return hm;
			else
				return new string[] { hm[2], hm[3], hm[0], hm[1], hm[4] };
		}

		private static string[] HalfMatch(string longText, string shortText, int i)
		{
			var seed = longText.Substring(i, longText.Length / 4);
			var j = -1;
			var bestCommon = string.Empty;
			var bestLongTextA = string.Empty;
			var bestLongTextB = string.Empty;
			var bestShortTextA = string.Empty;
			var bestShortTextB = string.Empty;

			while (j < shortText.Length && (j = shortText.IndexOf(seed, j + 1, StringComparison.Ordinal)) != -1)
			{
				var prefixLength = CommonPrefix(longText[i..], shortText[j..]);
				var suffixLength = CommonSuffix(longText.Substring(0, i), shortText.Substring(0, j));

				if (bestCommon.Length < suffixLength + prefixLength)
				{
					bestCommon = shortText.Substring(j - suffixLength, suffixLength) + shortText.Substring(j, prefixLength);
					bestLongTextA = longText.Substring(0, i - suffixLength);
					bestLongTextB = longText[(i + prefixLength)..];
					bestShortTextA = shortText.Substring(0, j - suffixLength);
					bestShortTextB = shortText[(j + prefixLength)..];
				}
			}

			if (bestCommon.Length * 2 >= longText.Length)
				return new string[] { bestLongTextA, bestLongTextB, bestShortTextA, bestShortTextB, bestCommon };
			else
				return null;
		}

		private static void CleanupSemantic(List<ITextDiffDescriptor> diffs)
		{
			var changes = false;
			var equalities = new Stack<int>();
			string lastEquality = null;
			var pointer = 0;
			var lengthInsertions1 = 0;
			var lengthDeletions1 = 0;
			var lengthInsertions2 = 0;
			var lengthDeletions2 = 0;

			while (pointer < diffs.Count)
			{
				if (diffs[pointer].Operation == TextDiffOperation.Equal)
				{
					equalities.Push(pointer);

					lengthInsertions1 = lengthInsertions2;
					lengthDeletions1 = lengthDeletions2;
					lengthInsertions2 = 0;
					lengthDeletions2 = 0;

					lastEquality = diffs[pointer].Text;
				}
				else
				{
					if (diffs[pointer].Operation == TextDiffOperation.Insert)
						lengthInsertions2 += diffs[pointer].Text.Length;
					else
						lengthDeletions2 += diffs[pointer].Text.Length;

					if (lastEquality != null && (lastEquality.Length <= Math.Max(lengthInsertions1, lengthDeletions1)) && (lastEquality.Length <= Math.Max(lengthInsertions2, lengthDeletions2)))
					{
						diffs.Insert(equalities.Peek(), new TextDiffDescriptor(TextDiffOperation.Delete, lastEquality));
						
						diffs[equalities.Peek() + 1].Operation = TextDiffOperation.Insert;

						equalities.Pop();

						if (equalities.Count > 0)
							equalities.Pop();

						pointer = equalities.Count > 0 ? equalities.Peek() : -1;
						lengthInsertions1 = 0;
						lengthDeletions1 = 0;
						lengthInsertions2 = 0;
						lengthDeletions2 = 0;
						lastEquality = null;
						changes = true;
					}
				}

				pointer++;
			}

			if (changes)
				CleanupMerge(diffs);

			CleanupSemanticLossless(diffs);

			pointer = 1;

			while (pointer < diffs.Count)
			{
				if (diffs[pointer - 1].Operation == TextDiffOperation.Delete && diffs[pointer].Operation == TextDiffOperation.Insert)
				{
					var deletion = diffs[pointer - 1].Text;
					var insertion = diffs[pointer].Text;
					var overlapLength1 = CommonOverlap(deletion, insertion);
					var overlapLength2 = CommonOverlap(insertion, deletion);

					if (overlapLength1 >= overlapLength2)
					{
						if (overlapLength1 >= deletion.Length / 2.0 || overlapLength1 >= insertion.Length / 2.0)
						{
							diffs.Insert(pointer, new TextDiffDescriptor(TextDiffOperation.Equal, insertion.Substring(0, overlapLength1)));

							diffs[pointer - 1].Text = deletion.Substring(0, deletion.Length - overlapLength1);
							diffs[pointer + 1].Text = insertion[overlapLength1..];

							pointer++;
						}
					}
					else
					{
						if (overlapLength2 >= deletion.Length / 2.0 || overlapLength2 >= insertion.Length / 2.0)
						{
							diffs.Insert(pointer, new TextDiffDescriptor(TextDiffOperation.Equal, deletion.Substring(0, overlapLength2)));

							diffs[pointer - 1].Operation = TextDiffOperation.Insert;
							diffs[pointer - 1].Text = insertion.Substring(0, insertion.Length - overlapLength2);
							diffs[pointer + 1].Operation = TextDiffOperation.Delete;
							diffs[pointer + 1].Text = deletion[overlapLength2..];

							pointer++;
						}
					}

					pointer++;
				}

				pointer++;
			}
		}

		private static void CleanupSemanticLossless(List<ITextDiffDescriptor> diffs)
		{
			var pointer = 1;

			while (pointer < diffs.Count - 1)
			{
				if (diffs[pointer - 1].Operation == TextDiffOperation.Equal && diffs[pointer + 1].Operation == TextDiffOperation.Equal)
				{
					var equality1 = diffs[pointer - 1].Text;
					var edit = diffs[pointer].Text;
					var equality2 = diffs[pointer + 1].Text;
					var commonOffset = CommonSuffix(equality1, edit);

					if (commonOffset > 0)
					{
						var commonString = edit[^commonOffset..];

						equality1 = equality1.Substring(0, equality1.Length - commonOffset);
						edit = commonString + edit.Substring(0, edit.Length - commonOffset);
						equality2 = commonString + equality2;
					}

					var bestEquality1 = equality1;
					var bestEdit = edit;
					var bestEquality2 = equality2;
					var bestScore = CleanupSemanticScore(equality1, edit) + CleanupSemanticScore(edit, equality2);

					while (edit.Length != 0 && equality2.Length != 0 && edit[0] == equality2[0])
					{
						equality1 += edit[0];
						edit = edit[1..] + equality2[0];
						equality2 = equality2[1..];
						var score = CleanupSemanticScore(equality1, edit) + CleanupSemanticScore(edit, equality2);

						if (score >= bestScore)
						{
							bestScore = score;
							bestEquality1 = equality1;
							bestEdit = edit;
							bestEquality2 = equality2;
						}
					}

					if (diffs[pointer - 1].Text != bestEquality1)
					{
						if (bestEquality1.Length != 0)
							diffs[pointer - 1].Text = bestEquality1;
						else
						{
							diffs.RemoveAt(pointer - 1);
							pointer--;
						}

						diffs[pointer].Text = bestEdit;

						if (bestEquality2.Length != 0)
							diffs[pointer + 1].Text = bestEquality2;
						else
						{
							diffs.RemoveAt(pointer + 1);
							pointer--;
						}
					}
				}

				pointer++;
			}
		}

		private static int CleanupSemanticScore(string one, string two)
		{
			if (one.Length == 0 || two.Length == 0)
				return 6;

			var char1 = one[^1];
			var char2 = two[0];
			var nonAlphaNumeric1 = !char.IsLetterOrDigit(char1);
			var nonAlphaNumeric2 = !char.IsLetterOrDigit(char2);
			var whitespace1 = nonAlphaNumeric1 && char.IsWhiteSpace(char1);
			var whitespace2 = nonAlphaNumeric2 && char.IsWhiteSpace(char2);
			var lineBreak1 = whitespace1 && char.IsControl(char1);
			var lineBreak2 = whitespace2 && char.IsControl(char2);
			var blankLine1 = lineBreak1 && BlankLineEnd.IsMatch(one);
			var blankLine2 = lineBreak2 && BlankLineStart.IsMatch(two);

			if (blankLine1 || blankLine2)
				return 5;
			else if (lineBreak1 || lineBreak2)
				return 4;
			else if (nonAlphaNumeric1 && !whitespace1 && whitespace2)
				return 3;
			else if (whitespace1 || whitespace2)
				return 2;
			else if (nonAlphaNumeric1 || nonAlphaNumeric2)
				return 1;

			return 0;
		}

		private static void CleanupEfficiency(List<ITextDiffDescriptor> diffs)
		{
			var changes = false;
			var equalities = new Stack<int>();
			var lastEquality = string.Empty;
			var pointer = 0;
			var preIns = false;
			var preDel = false;
			var postIns = false;
			var postDel = false;

			while (pointer < diffs.Count)
			{
				if (diffs[pointer].Operation == TextDiffOperation.Equal)
				{
					if (diffs[pointer].Text.Length < EditCost && (postIns || postDel))
					{
						equalities.Push(pointer);

						preIns = postIns;
						preDel = postDel;
						lastEquality = diffs[pointer].Text;
					}
					else
					{
						equalities.Clear();

						lastEquality = string.Empty;
					}

					postIns = postDel = false;
				}
				else
				{
					if (diffs[pointer].Operation == TextDiffOperation.Delete)
						postDel = true;
					else
						postIns = true;

					if ((lastEquality.Length != 0) && ((preIns && preDel && postIns && postDel) || ((lastEquality.Length < EditCost / 2) && ((preIns ? 1 : 0) + (preDel ? 1 : 0) + (postIns ? 1 : 0) + (postDel ? 1 : 0)) == 3)))
					{
						diffs.Insert(equalities.Peek(), new TextDiffDescriptor(TextDiffOperation.Delete, lastEquality));

						diffs[equalities.Peek() + 1].Operation = TextDiffOperation.Insert;

						equalities.Pop();
						lastEquality = string.Empty;

						if (preIns && preDel)
						{
							postIns = postDel = true;
							equalities.Clear();
						}
						else
						{
							if (equalities.Count > 0)
								equalities.Pop();

							pointer = equalities.Count > 0 ? equalities.Peek() : -1;
							postIns = postDel = false;
						}

						changes = true;
					}
				}

				pointer++;
			}

			if (changes)
				CleanupMerge(diffs);
		}

		private static void CleanupMerge(List<ITextDiffDescriptor> diffs)
		{
			diffs.Add(new TextDiffDescriptor(TextDiffOperation.Equal, string.Empty));

			var pointer = 0;
			var countDelete = 0;
			var countInsert = 0;
			var textDelete = string.Empty;
			var textInsert = string.Empty;
			var commonLength = 0;

			while (pointer < diffs.Count)
			{
				switch (diffs[pointer].Operation)
				{
					case TextDiffOperation.Insert:
						countInsert++;
						textInsert += diffs[pointer].Text;
						pointer++;
						break;
					case TextDiffOperation.Delete:
						countDelete++;
						textDelete += diffs[pointer].Text;
						pointer++;
						break;
					case TextDiffOperation.Equal:
						if (countDelete + countInsert > 1)
						{
							if (countDelete != 0 && countInsert != 0)
							{
								commonLength = CommonPrefix(textInsert, textDelete);

								if (commonLength != 0)
								{
									if ((pointer - countDelete - countInsert) > 0 && diffs[pointer - countDelete - countInsert - 1].Operation == TextDiffOperation.Equal)
										diffs[pointer - countDelete - countInsert - 1].Text += textInsert.Substring(0, commonLength);
									else
									{
										diffs.Insert(0, new TextDiffDescriptor(TextDiffOperation.Equal, textInsert.Substring(0, commonLength)));
										pointer++;
									}

									textInsert = textInsert[commonLength..];
									textDelete = textDelete[commonLength..];
								}

								commonLength = CommonSuffix(textInsert, textDelete);

								if (commonLength != 0)
								{
									diffs[pointer].Text = textInsert[^commonLength..] + diffs[pointer].Text;

									textInsert = textInsert.Substring(0, textInsert.Length - commonLength);
									textDelete = textDelete.Substring(0, textDelete.Length - commonLength);
								}
							}

							pointer -= countDelete + countInsert;
							Splice(diffs, pointer, countDelete + countInsert);

							if (textDelete.Length != 0)
							{
								Splice(diffs, pointer, 0, new TextDiffDescriptor(TextDiffOperation.Delete, textDelete));
								pointer++;
							}
							if (textInsert.Length != 0)
							{
								Splice(diffs, pointer, 0, new TextDiffDescriptor(TextDiffOperation.Insert, textInsert));
								pointer++;
							}
							pointer++;
						}
						else if (pointer != 0 && diffs[pointer - 1].Operation == TextDiffOperation.Equal)
						{
							diffs[pointer - 1].Text += diffs[pointer].Text;

							diffs.RemoveAt(pointer);
						}
						else
							pointer++;

						countInsert = 0;
						countDelete = 0;
						textDelete = string.Empty;
						textInsert = string.Empty;

						break;
				}
			}

			if (diffs[^1].Text.Length == 0)
				diffs.RemoveAt(diffs.Count - 1);

			var changes = false;
			pointer = 1;

			while (pointer < (diffs.Count - 1))
			{
				if (diffs[pointer - 1].Operation == TextDiffOperation.Equal && diffs[pointer + 1].Operation == TextDiffOperation.Equal)
				{
					if (diffs[pointer].Text.EndsWith(diffs[pointer - 1].Text, StringComparison.Ordinal))
					{
						diffs[pointer].Text = diffs[pointer - 1].Text + diffs[pointer].Text.Substring(0, diffs[pointer].Text.Length - diffs[pointer - 1].Text.Length);
						diffs[pointer + 1].Text = diffs[pointer - 1].Text + diffs[pointer + 1].Text;
						Splice(diffs, pointer - 1, 1);

						changes = true;
					}
					else if (diffs[pointer].Text.StartsWith(diffs[pointer + 1].Text, StringComparison.Ordinal))
					{
						diffs[pointer - 1].Text += diffs[pointer + 1].Text;
						diffs[pointer].Text = diffs[pointer].Text[diffs[pointer + 1].Text.Length..] + diffs[pointer + 1].Text;
						Splice(diffs, pointer + 1, 1);

						changes = true;
					}
				}

				pointer++;
			}

			if (changes)
				CleanupMerge(diffs);
		}

		private static int Index(List<ITextDiffDescriptor> diffs, int loc)
		{
			var chars1 = 0;
			var chars2 = 0;
			var lastChars1 = 0;
			var lastChars2 = 0;
			ITextDiffDescriptor lastDiff = null;

			foreach (var diff in diffs)
			{
				if (diff.Operation != TextDiffOperation.Insert)
					chars1 += diff.Text.Length;
				
				if (diff.Operation != TextDiffOperation.Delete)
					chars2 += diff.Text.Length;
				
				if (chars1 > loc)
				{
					lastDiff = diff;
					break;
				}

				lastChars1 = chars1;
				lastChars2 = chars2;
			}

			if (lastDiff != null && lastDiff.Operation == TextDiffOperation.Delete)
				return lastChars2;

			return lastChars2 + (loc - lastChars1);
		}

		public static string Render(List<ITextDiffDescriptor> diffs)
		{
			var html = new StringBuilder();

			foreach (var diff in diffs)
			{
				var text = diff.Text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\n", "&para;<br>");

				switch (diff.Operation)
				{
					case TextDiffOperation.Insert:
						html.Append("<ins style=\"background:#e6ffe6;\">").Append(text).Append("</ins>");
						break;
					case TextDiffOperation.Delete:
						html.Append("<del style=\"background:#ffe6e6;\">").Append(text).Append("</del>");
						break;
					case TextDiffOperation.Equal:
						html.Append("<span>").Append(text).Append("</span>");
						break;
				}
			}

			return html.ToString();
		}

		private static string Text1(List<ITextDiffDescriptor> diffs)
		{
			var text = new StringBuilder();

			foreach (var diff in diffs)
			{
				if (diff.Operation != TextDiffOperation.Insert)
					text.Append(diff.Text);
			}

			return text.ToString();
		}

		private static string Text2(List<ITextDiffDescriptor> diffs)
		{
			var text = new StringBuilder();

			foreach (var diff in diffs)
			{
				if (diff.Operation != TextDiffOperation.Delete)
					text.Append(diff.Text);
			}

			return text.ToString();
		}

		private static int Levenshtein(List<ITextDiffDescriptor> diffs)
		{
			var levenshtein = 0;
			var insertions = 0;
			var deletions = 0;

			foreach (var diff in diffs)
			{
				switch (diff.Operation)
				{
					case TextDiffOperation.Insert:
						insertions += diff.Text.Length;
						break;
					case TextDiffOperation.Delete:
						deletions += diff.Text.Length;
						break;
					case TextDiffOperation.Equal:
						levenshtein += Math.Max(insertions, deletions);

						insertions = 0;
						deletions = 0;
						break;
				}
			}

			levenshtein += Math.Max(insertions, deletions);

			return levenshtein;
		}

		private static string ToDelta(List<ITextDiffDescriptor> diffs)
		{
			var text = new StringBuilder();

			foreach (var diff in diffs)
			{
				switch (diff.Operation)
				{
					case TextDiffOperation.Insert:
						text.Append('+').Append(EncodeURI(diff.Text)).Append('\t');
						break;
					case TextDiffOperation.Delete:
						text.Append('-').Append(diff.Text.Length).Append('\t');
						break;
					case TextDiffOperation.Equal:
						text.Append('=').Append(diff.Text.Length).Append('\t');
						break;
				}
			}

			var delta = text.ToString();

			if (delta.Length != 0)
				delta = delta[0..^1];

			return delta;
		}

		private static List<ITextDiffDescriptor> FromDelta(string text1, string delta)
		{
			var diffs = new List<ITextDiffDescriptor>();
			var pointer = 0;
			var tokens = delta.Split(new string[] { "\t" }, StringSplitOptions.None);

			foreach (var token in tokens)
			{
				if (token.Length == 0)
					continue;

				var param = token[1..];

				switch (token[0])
				{
					case '+':
						param = param.Replace("+", "%2b");
						param = HttpUtility.UrlDecode(param);

						diffs.Add(new TextDiffDescriptor(TextDiffOperation.Insert, param));
						break;
					case '-':
					case '=':
						var n = 0;

						try
						{
							n = Convert.ToInt32(param);
						}
						catch (FormatException e)
						{
							throw new ArgumentException("Invalid number in diff_fromDelta: " + param, e);
						}

						if (n < 0)
							throw new ArgumentException("Negative number in diff_fromDelta: " + param);

						string text;

						try
						{
							text = text1.Substring(pointer, n);
							pointer += n;
						}
						catch (ArgumentOutOfRangeException e)
						{
							throw new ArgumentException("Delta length (" + pointer + ") larger than source text length (" + text1.Length + ").", e);
						}

						if (token[0] == '=')
							diffs.Add(new TextDiffDescriptor(TextDiffOperation.Equal, text));
						else
							diffs.Add(new TextDiffDescriptor(TextDiffOperation.Delete, text));

						break;
					default:
						throw new ArgumentException("Invalid diff operation in diff_fromDelta: " + token[0]);
				}
			}

			if (pointer != text1.Length)
				throw new ArgumentException("Delta length (" + pointer + ") smaller than source text length (" + text1.Length + ").");

			return diffs;
		}

		public static int Match(string text, string pattern, int loc)
		{
			loc = Math.Max(0, Math.Min(loc, text.Length));

			if (string.Compare(text, pattern, StringComparison.Ordinal) == 0)
				return 0;
			else if (text.Length == 0)
				return -1;
			else if (loc + pattern.Length <= text.Length && text.Substring(loc, pattern.Length) == pattern)
				return loc;
			else
				return MatchBitap(text, pattern, loc);
		}

		private static int MatchBitap(string text, string pattern, int loc)
		{
			var s = MatchAlphabet(pattern);
			var scoreThreshold = MatchThreshold;
			var bestLoc = text.IndexOf(pattern, loc, StringComparison.Ordinal);

			if (bestLoc != -1)
			{
				scoreThreshold = Math.Min(MatchBitapScore(0, bestLoc, loc, pattern), scoreThreshold);
				bestLoc = text.LastIndexOf(pattern, Math.Min(loc + pattern.Length, text.Length), StringComparison.Ordinal);

				if (bestLoc != -1)
					scoreThreshold = Math.Min(MatchBitapScore(0, bestLoc, loc, pattern), scoreThreshold);
			}

			var matchmask = 1 << (pattern.Length - 1);
			bestLoc = -1;

			int binMin;
			int binMid;
			var binMax = pattern.Length + text.Length;
			var lastRd = Array.Empty<int>();

			for (var i = 0; i < pattern.Length; i++)
			{
				binMin = 0;
				binMid = binMax;

				while (binMin < binMid)
				{
					if (MatchBitapScore(i, loc + binMid, loc, pattern) <= scoreThreshold)
						binMin = binMid;
					else
						binMax = binMid;

					binMid = (binMax - binMin) / 2 + binMin;
				}

				binMax = binMid;

				var start = Math.Max(1, loc - binMid + 1);
				var finish = Math.Min(loc + binMid, text.Length) + pattern.Length;
				var rd = new int[finish + 2];

				rd[finish + 1] = (1 << i) - 1;

				for (var j = finish; j >= start; j--)
				{
					int charMatch;

					if (text.Length <= j - 1 || !s.ContainsKey(text[j - 1]))
						charMatch = 0;
					else
						charMatch = s[text[j - 1]];
					if (i == 0)
						rd[j] = ((rd[j + 1] << 1) | 1) & charMatch;
					else
						rd[j] = ((rd[j + 1] << 1) | 1) & charMatch | (((lastRd[j + 1] | lastRd[j]) << 1) | 1) | lastRd[j + 1];
					if ((rd[j] & matchmask) != 0)
					{
						var score = MatchBitapScore(i, j - 1, loc, pattern);

						if (score <= scoreThreshold)
						{
							scoreThreshold = score;
							bestLoc = j - 1;
							if (bestLoc > loc)
								start = Math.Max(1, 2 * loc - bestLoc);
							else
								break;
						}
					}
				}
				if (MatchBitapScore(i + 1, loc, loc, pattern) > scoreThreshold)
					break;

				lastRd = rd;
			}

			return bestLoc;
		}

		private static double MatchBitapScore(int e, int x, int loc, string pattern)
		{
			var accuracy = (float)e / pattern.Length;
			var proximity = Math.Abs(loc - x);

			return accuracy + (proximity / (float)MatchDistance);
		}

		private static Dictionary<char, int> MatchAlphabet(string pattern)
		{
			var s = new Dictionary<char, int>();
			var charPattern = pattern.ToCharArray();

			foreach (var c in charPattern)
			{
				if (!s.ContainsKey(c))
					s.Add(c, 0);
			}

			var i = 0;

			foreach (char c in charPattern)
			{
				var value = s[c] | (1 << (pattern.Length - i - 1));
				s[c] = value;
				i++;
			}

			return s;
		}


		private static void AddContext(ITextPatchDescriptor patch, string text)
		{
			if (text.Length == 0)
				return;

			var pattern = text.Substring(patch.Start2, patch.Length1);
			var padding = 0;

			while (text.IndexOf(pattern, StringComparison.Ordinal) != text.LastIndexOf(pattern, StringComparison.Ordinal) && pattern.Length < MatchMaxBits - PatchMargin - PatchMargin)
			{
				padding += PatchMargin;
				pattern = text[Math.Max(0, patch.Start2 - padding)..Math.Min(text.Length, patch.Start2 + patch.Length1 + padding)];
			}

			padding += PatchMargin;

			var prefix = text[Math.Max(0, patch.Start2 - padding)..patch.Start2];

			if (prefix.Length != 0)
				patch.Diffs.Insert(0, new TextDiffDescriptor(TextDiffOperation.Equal, prefix));

			string suffix = text[(patch.Start2 + patch.Length1)..Math.Min(text.Length, patch.Start2 + patch.Length1 + padding)];

			if (suffix.Length != 0)
				patch.Diffs.Add(new TextDiffDescriptor(TextDiffOperation.Equal, suffix));

			patch.Start1 -= prefix.Length;
			patch.Start2 -= prefix.Length;
			patch.Length1 += prefix.Length + suffix.Length;
			patch.Length2 += prefix.Length + suffix.Length;
		}

		private static List<ITextPatchDescriptor> Patch(TextDiffArgs e)
		{
			var diffs = Diff(e, true);

			if (diffs.Count > 2)
			{
				CleanupSemantic(diffs);
				CleanupEfficiency(diffs);
			}

			return Patch(e.Original, diffs);
		}

		public static List<ITextPatchDescriptor> Patch(List<ITextDiffDescriptor> diffs)
		{
			var text1 = Text1(diffs);
			return Patch(text1, diffs);
		}

		private static List<ITextPatchDescriptor> Patch(string text1, List<ITextDiffDescriptor> diffs)
		{
			var patches = new List<ITextPatchDescriptor>();

			if (diffs.Count == 0)
				return patches;

			var patch = new TextPatchDescriptor();
			var charCount1 = 0;
			var charCount2 = 0;
			var prepatchText = text1;
			var postpatchText = text1;

			foreach (var diff in diffs)
			{
				if (patch.Diffs.Count == 0 && diff.Operation != TextDiffOperation.Equal)
				{
					patch.Start1 = charCount1;
					patch.Start2 = charCount2;
				}

				switch (diff.Operation)
				{
					case TextDiffOperation.Insert:
						patch.Diffs.Add(diff);
						patch.Length2 += diff.Text.Length;
						postpatchText = postpatchText.Insert(charCount2, diff.Text);
						break;
					case TextDiffOperation.Delete:
						patch.Length1 += diff.Text.Length;
						patch.Diffs.Add(diff);
						postpatchText = postpatchText.Remove(charCount2,
							 diff.Text.Length);
						break;
					case TextDiffOperation.Equal:
						if (diff.Text.Length <= 2 * PatchMargin
							 && patch.Diffs.Count != 0 && diff != diffs.Last())
						{
							patch.Diffs.Add(diff);
							patch.Length1 += diff.Text.Length;
							patch.Length2 += diff.Text.Length;
						}

						if (diff.Text.Length >= 2 * PatchMargin)
						{
							if (patch.Diffs.Count != 0)
							{
								AddContext(patch, prepatchText);
								patches.Add(patch);
								patch = new TextPatchDescriptor();

								prepatchText = postpatchText;
								charCount1 = charCount2;
							}
						}
						break;
				}

				if (diff.Operation != TextDiffOperation.Insert)
					charCount1 += diff.Text.Length;

				if (diff.Operation != TextDiffOperation.Delete)
					charCount2 += diff.Text.Length;
			}

			if (patch.Diffs.Count != 0)
			{
				AddContext(patch, prepatchText);
				patches.Add(patch);
			}

			return patches;
		}

		private static List<ITextPatchDescriptor> DeepCopy(List<ITextPatchDescriptor> patches)
		{
			var patchesCopy = new List<ITextPatchDescriptor>();

			foreach (var patch in patches)
			{
				var patchCopy = new TextPatchDescriptor();

				foreach (var diff in patch.Diffs)
				{
					var diffCopy = new TextDiffDescriptor(diff.Operation, diff.Text);

					patchCopy.Diffs.Add(diffCopy);
				}

				patchCopy.Start1 = patch.Start1;
				patchCopy.Start2 = patch.Start2;
				patchCopy.Length1 = patch.Length1;
				patchCopy.Length2 = patch.Length2;

				patchesCopy.Add(patchCopy);
			}

			return patchesCopy;
		}

		public static ITextPatchResult Apply(List<ITextPatchDescriptor> patches, string text)
		{
			if (patches.Count == 0)
			{
				return new TextPatchResult
				{
					Text = text
				};
			}

			patches = DeepCopy(patches);

			var nullPadding = AddPadding(patches);

			text = nullPadding + text + nullPadding;
			SplitMax(patches);

			var delta = 0;
			var results = new TextPatchResult();

			foreach (var patch in patches)
			{
				var success = true;
				var expectedLoc = patch.Start2 + delta;
				var text1 = Text1(patch.Diffs);
				int startLoc;
				var endLoc = -1;

				if (text1.Length > MatchMaxBits)
				{
					startLoc = Match(text, text1.Substring(0, MatchMaxBits), expectedLoc);

					if (startLoc != -1)
					{
						endLoc = Match(text, text1[^MatchMaxBits..], expectedLoc + text1.Length - MatchMaxBits);

						if (endLoc == -1 || startLoc >= endLoc)
							startLoc = -1;
					}
				}
				else
					startLoc = Match(text, text1, expectedLoc);

				if (startLoc == -1)
				{
					success = false;
					delta -= patch.Length2 - patch.Length1;
				}
				else
				{
					success = true;
					delta = startLoc - expectedLoc;
					string text2;

					if (endLoc == -1)
						text2 = text[startLoc..Math.Min(startLoc + text1.Length, text.Length)];
					else
						text2 = text[startLoc..Math.Min(endLoc + MatchMaxBits, text.Length)];

					if (string.Compare(text1, text2, StringComparison.Ordinal) == 0)
						text = text.Substring(0, startLoc) + Text2(patch.Diffs) + text[(startLoc + text1.Length)..];
					else
					{
						var diffs = Diff(new TextDiffArgs
						{
							Original = text1,
							Modified = text2,
							Timeout = 0,
							Mode = TextDiffCompareMode.Char
						}, false);

						if (text1.Length > MatchMaxBits && Levenshtein(diffs) / (float)text1.Length > PatchDeleteThreshold)
							success = false;
						else
						{
							CleanupSemanticLossless(diffs);
							var index1 = 0;

							foreach (var diff in patch.Diffs)
							{
								if (diff.Operation != TextDiffOperation.Equal)
								{
									var index2 = Index(diffs, index1);

									if (diff.Operation == TextDiffOperation.Insert)
										text = text.Insert(startLoc + index2, diff.Text);
									else if (diff.Operation == TextDiffOperation.Delete)
										text = text.Remove(startLoc + index2, Index(diffs, index1 + diff.Text.Length) - index2);
								}

								if (diff.Operation != TextDiffOperation.Delete)
									index1 += diff.Text.Length;
							}
						}
					}
				}

				results.Patches.Add(success);
			}

			results.Text = text.Substring(nullPadding.Length, text.Length - 2 * nullPadding.Length);

			return results;
		}

		private static string AddPadding(List<ITextPatchDescriptor> patches)
		{
			var paddingLength = PatchMargin;
			var nullPadding = string.Empty;

			for (var x = 1; x <= paddingLength; x++)
				nullPadding += (char)x;

			foreach (var p in patches)
			{
				p.Start1 += paddingLength;
				p.Start2 += paddingLength;
			}

			var patch = patches.First();
			var diffs = patch.Diffs;

			if (diffs.Count == 0 || diffs.First().Operation != TextDiffOperation.Equal)
			{
				diffs.Insert(0, new TextDiffDescriptor(TextDiffOperation.Equal, nullPadding));
				patch.Start1 -= paddingLength;
				patch.Start2 -= paddingLength;
				patch.Length1 += paddingLength;
				patch.Length2 += paddingLength;
			}
			else if (paddingLength > diffs.First().Text.Length)
			{
				var firstDiff = diffs.First();
				var extraLength = paddingLength - firstDiff.Text.Length;

				firstDiff.Text = nullPadding[firstDiff.Text.Length..] + firstDiff.Text;
				patch.Start1 -= extraLength;
				patch.Start2 -= extraLength;
				patch.Length1 += extraLength;
				patch.Length2 += extraLength;
			}

			patch = patches.Last();
			diffs = patch.Diffs;

			if (diffs.Count == 0 || diffs.Last().Operation != TextDiffOperation.Equal)
			{
				diffs.Add(new TextDiffDescriptor(TextDiffOperation.Equal, nullPadding));

				patch.Length1 += paddingLength;
				patch.Length2 += paddingLength;
			}
			else if (paddingLength > diffs.Last().Text.Length)
			{
				var lastDiff = diffs.Last();
				var extraLength = paddingLength - lastDiff.Text.Length;

				lastDiff.Text += nullPadding.Substring(0, extraLength);

				patch.Length1 += extraLength;
				patch.Length2 += extraLength;
			}

			return nullPadding;
		}

		private static void SplitMax(List<ITextPatchDescriptor> patches)
		{
			var patchSize = MatchMaxBits;

			for (var i = 0; i < patches.Count; i++)
			{
				if (patches[i].Length1 <= patchSize)
					continue;

				var bigPatch = patches[i];

				Splice(patches, i--, 1);

				var start1 = bigPatch.Start1;
				var start2 = bigPatch.Start2;
				var precontext = string.Empty;

				while (bigPatch.Diffs.Count != 0)
				{
					var patch = new TextPatchDescriptor();
					var empty = true;

					patch.Start1 = start1 - precontext.Length;
					patch.Start2 = start2 - precontext.Length;

					if (precontext.Length != 0)
					{
						patch.Length1 = patch.Length2 = precontext.Length;
						patch.Diffs.Add(new TextDiffDescriptor(TextDiffOperation.Equal, precontext));
					}

					while (bigPatch.Diffs.Count != 0 && patch.Length1 < patchSize - PatchMargin)
					{
						var diffType = bigPatch.Diffs[0].Operation;
						var diffText = bigPatch.Diffs[0].Text;

						if (diffType == TextDiffOperation.Insert)
						{
							patch.Length2 += diffText.Length;
							start2 += diffText.Length;
							patch.Diffs.Add(bigPatch.Diffs.First());
							bigPatch.Diffs.RemoveAt(0);
							empty = false;
						}
						else if (diffType == TextDiffOperation.Delete && patch.Diffs.Count == 1 && patch.Diffs.First().Operation == TextDiffOperation.Equal && diffText.Length > 2 * patchSize)
						{
							patch.Length1 += diffText.Length;
							start1 += diffText.Length;
							empty = false;
							patch.Diffs.Add(new TextDiffDescriptor(diffType, diffText));
							bigPatch.Diffs.RemoveAt(0);
						}
						else
						{
							diffText = diffText.Substring(0, Math.Min(diffText.Length, patchSize - patch.Length1 - PatchMargin));
							patch.Length1 += diffText.Length;
							start1 += diffText.Length;

							if (diffType == TextDiffOperation.Equal)
							{
								patch.Length2 += diffText.Length;
								start2 += diffText.Length;
							}
							else
								empty = false;

							patch.Diffs.Add(new TextDiffDescriptor(diffType, diffText));

							if (diffText == bigPatch.Diffs[0].Text)
								bigPatch.Diffs.RemoveAt(0);
							else
								bigPatch.Diffs[0].Text = bigPatch.Diffs[0].Text[diffText.Length..];
						}
					}

					precontext = Text2(patch.Diffs);
					precontext = precontext[Math.Max(0, precontext.Length - PatchMargin)..];

					string postcontext = null;

					if (Text1(bigPatch.Diffs).Length > PatchMargin)
						postcontext = Text1(bigPatch.Diffs).Substring(0, PatchMargin);
					else
						postcontext = Text1(bigPatch.Diffs);

					if (postcontext.Length != 0)
					{
						patch.Length1 += postcontext.Length;
						patch.Length2 += postcontext.Length;

						if (patch.Diffs.Count != 0 && patch.Diffs[^1].Operation == TextDiffOperation.Equal)
							patch.Diffs[^1].Text += postcontext;
						else
							patch.Diffs.Add(new TextDiffDescriptor(TextDiffOperation.Equal, postcontext));
					}

					if (!empty)
						Splice(patches, ++i, 0, patch);
				}
			}
		}

		public static string ToText(List<ITextPatchDescriptor> patches)
		{
			var text = new StringBuilder();

			foreach (var patch in patches)
				text.Append(patch);

			return text.ToString();
		}

		private static List<ITextPatchDescriptor> FromText(string textline)
		{
			var patches = new List<ITextPatchDescriptor>();

			if (textline.Length == 0)
				return patches;

			var text = textline.Split('\n');
			var textPointer = 0;
			ITextPatchDescriptor patch;
			Regex patchHeader = new Regex("^@@ -(\\d+),?(\\d*) \\+(\\d+),?(\\d*) @@$");
			Match m;
			char sign;
			string line;

			while (textPointer < text.Length)
			{
				m = patchHeader.Match(text[textPointer]);

				if (!m.Success)
					throw new ArgumentException("Invalid patch string: " + text[textPointer]);

				patch = new TextPatchDescriptor();
				patches.Add(patch);
				patch.Start1 = Convert.ToInt32(m.Groups[1].Value);

				if (m.Groups[2].Length == 0)
				{
					patch.Start1--;
					patch.Length1 = 1;
				}
				else if (m.Groups[2].Value == "0")
					patch.Length1 = 0;
				else
				{
					patch.Start1--;
					patch.Length1 = Convert.ToInt32(m.Groups[2].Value);
				}

				patch.Start2 = Convert.ToInt32(m.Groups[3].Value);

				if (m.Groups[4].Length == 0)
				{
					patch.Start2--;
					patch.Length2 = 1;
				}
				else if (m.Groups[4].Value == "0")
					patch.Length2 = 0;
				else
				{
					patch.Start2--;
					patch.Length2 = Convert.ToInt32(m.Groups[4].Value);
				}

				textPointer++;

				while (textPointer < text.Length)
				{
					try
					{
						sign = text[textPointer][0];
					}
					catch (IndexOutOfRangeException)
					{
						textPointer++;
						continue;
					}

					line = text[textPointer][1..];
					line = line.Replace("+", "%2b");
					line = HttpUtility.UrlDecode(line);

					if (sign == '-')
						patch.Diffs.Add(new TextDiffDescriptor(TextDiffOperation.Delete, line));
					else if (sign == '+')
						patch.Diffs.Add(new TextDiffDescriptor(TextDiffOperation.Insert, line));
					else if (sign == ' ')
						patch.Diffs.Add(new TextDiffDescriptor(TextDiffOperation.Equal, line));
					else if (sign == '@')
						break;
					else
						throw new ArgumentException("Invalid patch mode '" + sign + "' in: " + line);

					textPointer++;
				}
			}

			return patches;
		}

		public static string EncodeURI(string value)
		{
			return new StringBuilder(HttpUtility.UrlEncode(value))
				 .Replace('+', ' ').Replace("%20", " ").Replace("%21", "!")
				 .Replace("%2a", "*").Replace("%27", "'").Replace("%28", "(")
				 .Replace("%29", ")").Replace("%3b", ";").Replace("%2f", "/")
				 .Replace("%3f", "?").Replace("%3a", ":").Replace("%40", "@")
				 .Replace("%26", "&").Replace("%3d", "=").Replace("%2b", "+")
				 .Replace("%24", "$").Replace("%2c", ",").Replace("%23", "#")
				 .Replace("%7e", "~")
				 .ToString();
		}

		private static List<T> Splice<T>(List<T> input, int start, int count, params T[] args)
		{
			var deletedRange = input.GetRange(start, count);

			input.RemoveRange(start, count);
			input.InsertRange(start, args);

			return deletedRange;
		}
	}
}
