#if WORKINPROGRESS
#if MIGRATION
using CSHTML5.Internal;
using System.Windows.Controls;
using System.Windows.Media;

namespace System.Windows
#else
namespace Windows.UI.Xaml
#endif
{
	public interface ITextMeasurementService
	{
		Size Measure(string text, double fontSize, FontFamily fontFamily, FontStyle style, FontWeight weight, FontStretch stretch, Thickness padding, double maxWidth);
        void CreateMeasurementText(UIElement parent);
        string GetTextMeasureDivID();
    }

    //
    // Summary:
    //     Measure Text Block width and height from html element.
    public class TextMeasurementService : ITextMeasurementService
    {
#if CSHTML5NETSTANDARD
        private INTERNAL_HtmlDomStyleReference
#else
        private dynamic
#endif
            textDivStyle;

        private object textDivReference;
        private TextBox associatedTextUI;
        private string textDivUniqueIdentifier;

        public TextMeasurementService()
        {
            textDivUniqueIdentifier = "";
        }

        public void CreateMeasurementText(UIElement parent)
        {
            associatedTextUI = new TextBox();
#if REWORKLOADED
                this.AddVisualChild(associatedUI);
#else
            INTERNAL_VisualTreeManager.AttachVisualChildIfNotAlreadyAttached(associatedTextUI, parent);
#endif
            textDivReference = (INTERNAL_HtmlDomElementReference)associatedTextUI.INTERNAL_OuterDomElement;
            textDivStyle = INTERNAL_HtmlDomManager.GetDomElementStyleForModification(textDivReference);

            textDivStyle.position = "absolute";
            //textDivStyle.visibility = "hidden";
            textDivStyle.height = "";
            textDivStyle.width = "";
            textDivStyle.top = "0px";

            textDivUniqueIdentifier = ((INTERNAL_HtmlDomElementReference)textDivReference).UniqueIdentifier;
        }

        public string GetTextMeasureDivID()
        {
            return textDivUniqueIdentifier;
        }   

        public Size Measure(string text, double fontSize, FontFamily fontFamily, FontStyle style, FontWeight weight, FontStretch stretch, Thickness padding, double maxWidth)
        {
            //Console.WriteLine($"TextMeasurementService maxWidth {maxWidth}");

            if (textDivReference == null)
            {
                return Size.Zero;
            }

            associatedTextUI.Text = String.IsNullOrEmpty(text) ? "A" : text;
            associatedTextUI.FontFamily = fontFamily;
            associatedTextUI.FontStyle = style;
            associatedTextUI.FontWeight = weight;
            associatedTextUI.FontStretch = stretch;
            associatedTextUI.Padding = padding;
            associatedTextUI.FontSize = fontSize;

            if (maxWidth.IsNaN() || double.IsInfinity(maxWidth))
            {
                associatedTextUI.TextWrapping = TextWrapping.NoWrap;
                textDivStyle.width = "";
                textDivStyle.maxWidth = "";
            }
            else
            {
                associatedTextUI.TextWrapping = TextWrapping.Wrap;
                textDivStyle.width = String.Format("{0}px", maxWidth);
                textDivStyle.maxWidth = String.Format("{0}px", maxWidth);
            }

            return new Size(associatedTextUI.ActualWidth + 1, associatedTextUI.ActualHeight);
        }
    }
}
#endif