namespace piconavx.ui.graphics.ui
{
    public class Navigator
    {
        private Stack<Page> history;
        private Page? currentPage = null;

        public Page? CurrentPage => currentPage;

        public Navigator()
        {
            history = new Stack<Page>();
        }

        public void Push(Page page)
        {
            if (Scene.InEvent)
            {
                Scene.InvokeLater(() => Push(page), DeferralMode.NextFrame); // defer execution until next frame
                return;
            }

            page.Show();
            if (currentPage != null)
            {
                history.Push(currentPage);
                currentPage.Hide();
            }
            currentPage = page;
            Scene.ComplainLayoutShift(2);
        }

        public void Replace(Page page)
        {
            if (Scene.InEvent)
            {
                Scene.InvokeLater(() => Replace(page), DeferralMode.NextFrame); // defer execution until next frame
                return;
            }

            page.Show();
            currentPage?.Hide();
            currentPage = page;
            Scene.ComplainLayoutShift(2);
        }

        public void Back()
        {
            if (Scene.InEvent)
            {
                Scene.InvokeLater(Back, DeferralMode.NextFrame); // defer execution until next frame
                return;
            }

            if (history.TryPop(out Page? page))
            {
                page.Show();
                currentPage?.Hide();
                currentPage = page;
                Scene.ComplainLayoutShift(2);
            }
        }

        public bool RemoveIncluding(Page page)
        {
            while (history.TryPop(out Page? _page))
            {
                if (_page.Equals(page))
                    return true;
            }

            return false;
        }
    }
}
