using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace YYMP
{
    //https://takopp.wordpress.com/2012/11/26/wpf-%E6%95%B0%E5%80%A4%E4%BB%A5%E5%A4%96%E3%81%AE%E5%85%A5%E5%8A%9B%E3%81%8C%E3%81%A7%E3%81%8D%E3%81%AA%E3%81%84textbox/
    public class NumberTextBox : TextBox
    {
        static NumberTextBox()
        {
            // IMEを無効化。
            InputMethod.IsInputMethodEnabledProperty.OverrideMetadata(typeof(NumberTextBox), new FrameworkPropertyMetadata(false));
        }

        public NumberTextBox()
        {
            // ペーストのキーボードショートカットを無効化。
            this.InputBindings.Add(new KeyBinding(ApplicationCommands.NotACommand, Key.V, ModifierKeys.Control));
            this.InputBindings.Add(new KeyBinding(ApplicationCommands.NotACommand, Key.Insert, ModifierKeys.Shift));
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            // 数値以外、または数値の入力に関係しないキーが押された場合、イベントを処理済みに。
            if (!((Key.D0 <= e.Key && e.Key <= Key.D9) ||
                  (Key.NumPad0 <= e.Key && e.Key <= Key.NumPad9) ||
                  Key.Back == e.Key ||
                  Key.Delete == e.Key ||
                  Key.Tab == e.Key) ||
                (Keyboard.Modifiers & ModifierKeys.Shift) > 0)
            {
                e.Handled = true;
            }

            base.OnKeyDown(e);
        }

        // 右クリックを無効化。
        protected override void OnPreviewMouseRightButtonUp(MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}