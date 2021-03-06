﻿

/*===================================================================================
* 
*   Copyright (c) Userware/OpenSilver.net
*      
*   This file is part of the OpenSilver Runtime (https://opensilver.net), which is
*   licensed under the MIT license: https://opensource.org/licenses/MIT
*   
*   As stated in the MIT license, "the above copyright notice and this permission
*   notice shall be included in all copies or substantial portions of the Software."
*  
\*====================================================================================*/


using CSHTML5.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if MIGRATION
using System.Windows.Media;
#else
using Windows.UI.Xaml.Media;
#endif

#if !BRIDGE
using JSIL.Meta;
#else
using Bridge;
#endif

#if MIGRATION
namespace System.Windows.Controls
#else
namespace Windows.UI.Xaml.Controls
#endif
{
    /// <summary>
    /// Represents a control for entering passwords.
    /// </summary>
    public partial class PasswordBox
    {
        /// <summary>
        /// Identifies the <see cref="PasswordBox.SelectionForeground"/> dependency
        /// property.
        /// </summary>
        [OpenSilver.NotImplemented]
        public static readonly DependencyProperty SelectionForegroundProperty = 
            DependencyProperty.Register(
                nameof(SelectionForeground), 
                typeof(Brush), 
                typeof(PasswordBox), 
                new PropertyMetadata((object)null));

        /// <summary>
        /// Gets or sets the brush used for the selected text in the <see cref="PasswordBox"/>.
        /// </summary>
        [OpenSilver.NotImplemented]
        public Brush SelectionForeground
        {
            get { return (Brush)this.GetValue(SelectionForegroundProperty); }
            set { this.SetValue(SelectionForegroundProperty, value); }
        }

    }
}
