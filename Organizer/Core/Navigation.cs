using Organizer.View;
using System;
using System.Windows.Controls;

namespace Organizer.Core
{
    public class Navigation
    {
        private readonly Frame _frame;
        public string Title { get; set; }

        public Navigation(Frame frame)
        {
            _frame = frame;
        }

        public void NavigateTo(object param, object dataContext)
        {
            string pageName = param.ToString();
            Page page;

            switch (pageName)
            {
                case "EditTask":
                    page = new EditPage();
                    break;
                case "MainPage":
                    page = new MainOrganizerPage();
                    break;
                case "AddNewTask":
                    page = new AddNewTask();
                    break;
                default:
                    throw new ArgumentException("Неизвестная страница ", nameof(pageName));
            }

            page.DataContext = dataContext;
            _frame.Navigate(page);
        }
    }
}
