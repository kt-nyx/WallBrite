namespace WallBrite
{
    public class AddFileProgressViewModel
    {
        public LibraryViewModel Library { get; set; }

        private readonly AddFileProgressWindow _progressWindow;
        public AddFileProgressViewModel(LibraryViewModel library)
        {
            Library = library;

            _progressWindow = new AddFileProgressWindow
            {
                DataContext = this
            };

            _progressWindow.Show();
        }

        public void CloseWindow()
        {
            _progressWindow.Close();
        }

    }
}
