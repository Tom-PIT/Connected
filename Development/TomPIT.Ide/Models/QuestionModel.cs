namespace TomPIT.Models
{
	public class QuestionModel
	{
		public string Id { get; set; }
		public string Message { get; set; }

		public string ConfirmText { get; set; } = "Confirm";
		public string CancelText { get; set; } = "Cancel";
	}
}
