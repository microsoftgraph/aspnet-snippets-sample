namespace SnippetsApp.Models
{
    // Represents a Bootstrap alert
    public class Alert
    {
        // "success", "error", "info", etc.
        public string Type { get; set; }

        // The main text to display in the alert
        public string Message { get; set; }

        // Optional debug information, rendered as
        // preformatted code
        public string DebugInfo { get; set; }

        // If the alert contains a button, this
        // is the text on the button
        public string ActionText { get; set; }

        // If the alert contains a button, this is
        // the href for the button
        public string ActionUrl { get; set; }
    }
}

