namespace WallBrite
{
    public class AddFileProgressViewModel
    {
        public LibraryViewModel Library { get; set; }

        public AddFileProgressWindow ProgressWindow { get; private set; }

        public AddFileProgressViewModel(LibraryViewModel library)
        {
            Library = library;

            ProgressWindow = new AddFileProgressWindow
            {
                DataContext = this
            };

            ProgressWindow.Show();
        }

        public void CloseWindow()
        {
            ProgressWindow.Close();
        }

    }
}
