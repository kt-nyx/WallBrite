namespace WallBrite
{
    /// <summary>
    /// VM representing the "Add Progress" window, which gives the user info on the progress of adding new
    /// files to the library
    /// </summary>
    public class AddFileProgressViewModel
    {
        /// <summary>
        /// The library being manipulated
        /// </summary>
        public LibraryViewModel Library { get; set; }

        /// <summary>
        /// The relevant window
        /// </summary>
        public AddFileProgressWindow ProgressWindow { get; private set; }

        /// <summary>
        /// Creates an AddFileProgressViewModel using the given LibraryViewModel; sets data context and opens
        /// the window
        /// </summary>
        /// <param name="library">The relevant library</param>
        public AddFileProgressViewModel(LibraryViewModel library)
        {
            Library = library;

            ProgressWindow = new AddFileProgressWindow
            {
                DataContext = this
            };

            ProgressWindow.Show();
        }

        /// <summary>
        /// Closes the ProgressWindow
        /// </summary>
        public void CloseWindow()
        {
            ProgressWindow.Close();
        }

    }
}
