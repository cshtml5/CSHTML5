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
using OpenSilver.Internal.Controls;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Windows.Markup;
using System.Diagnostics;
using CSHTML5.Internals.Controls;

#if MIGRATION
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
#else
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
#endif

#if MIGRATION
namespace System.Windows.Controls
#else
namespace Windows.UI.Xaml.Controls
#endif
{
    /// <summary>
    /// Represents a control that can be used to present a collection of items.
    /// </summary>
    [ContentProperty("Items")]
    public partial class ItemsControl : Control, IGeneratorHost
    {
        #region Data

        // Note: this maps an item (for example a string) to the element
        // that is added to the visual tree (such a datatemplate) or to 
        // the native DOM element in case of native combo box for example.
        private ItemContainerGenerator _itemContainerGenerator;

        // ItemsPresenter retrieve from this control's Template
        // in which items will be rendered.
        internal ItemsPresenter _itemsPresenter;

        private ItemCollection _items;

        #endregion Data

        #region Contructor

        /// <summary>
        /// Initializes a new instance of the ItemsControl class.
        /// </summary>
        public ItemsControl()
        {
            this.DefaultStyleKey = typeof(ItemsControl);
        }

        #endregion Constructor

        #region Public Properties

        /// <summary>
        /// Items is the collection of data that is used to generate the content
        /// of this control.
        /// </summary>
        public ItemCollection Items
        {
            get
            {
                if (this._items == null)
                {
                    this.CreateItemCollectionAndGenerator();
                }
                return this._items;
            }
        }

        /// <summary>
        /// Gets the ItemContainerGenerator associated with this ItemsControl.
        /// </summary>
        public ItemContainerGenerator ItemContainerGenerator
        {
            get
            {
                if (this._itemContainerGenerator == null)
                {
                    this.CreateItemCollectionAndGenerator();
                }
                return _itemContainerGenerator;
            }
        }

        private void CreateItemCollectionAndGenerator()
        {
            this._items = new ItemCollection(this);

            // the generator must attach its collection change handler before
            // the control itself, so that the generator is up-to-date by the
            // time the control tries to use it
            this._itemContainerGenerator = new ItemContainerGenerator();

            this._items.CollectionChanged += this.OnItemCollectionChanged;
        }

        #endregion Public Properties

        #region Dependency Properties

        /// <summary>
        /// Gets or sets the template that defines the panel that controls the layout
        /// of items.
        /// </summary>
        public ItemsPanelTemplate ItemsPanel
        {
            get { return (ItemsPanelTemplate)GetValue(ItemsPanelProperty); }
            set { SetValue(ItemsPanelProperty, value); }
        }

        /// <summary>
        /// Identifies the ItemsPanel dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemsPanelProperty =
            DependencyProperty.Register(
                nameof(ItemsPanel),
                typeof(ItemsPanelTemplate),
                typeof(ItemsControl),
                new PropertyMetadata(GetDefaultItemsPanel(), OnItemsPanelChanged));

        private static ItemsPanelTemplate GetDefaultItemsPanel()
        {
            ItemsPanelTemplate template = new ItemsPanelTemplate()
            {
                _methodToInstantiateFrameworkTemplate = (FrameworkElement templateOwner) =>
                {
                    return new TemplateInstance()
                    {
                        TemplateOwner = templateOwner,
                        TemplateContent = new StackPanel()
                    };
                }
            };
            template.Seal();

            // Note: We seal the template in order to avoid letting the user modify the 
            // default template itself since it is the same instance that is used as 
            // the default value for all ItemsControls.
            // This would bring issues such as a user modifying the default template 
            // for one element then modifying it again for another one and both would 
            // have the last one's template.
            return template;
        }

        private static void OnItemsPanelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ItemsControl)d).UpdateItemsPanel((ItemsPanelTemplate)e.NewValue);
        }

        /// <summary>
        /// Gets or sets an object source used to generate the content of the ItemsControl.
        /// </summary>
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        /// <summary>
        /// Identifies the ItemsSource dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                nameof(ItemsSource),
                typeof(IEnumerable),
                typeof(ItemsControl),
                new PropertyMetadata(null, OnItemsSourceChanged));

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ItemsControl ic = (ItemsControl)d;
            IEnumerable oldValue = (IEnumerable)e.OldValue;
            IEnumerable newValue = (IEnumerable)e.NewValue;

            if (e.NewValue != null)
            {
                // ItemsSource is non-null.  Go to ItemsSource mode
                ic.Items.SetItemsSource(newValue);
            }
            else
            {
                // ItemsSource is explicitly null.  Return to normal mode.
                ic.Items.ClearItemsSource();
            }

            ic.OnItemsSourceChanged(oldValue, newValue);
        }

        /// <summary>
        /// Gets or sets the DataTemplate used to display each item.
        /// </summary>
        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        /// <summary>
        /// Identifies the ItemTemplate dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register(
                nameof(ItemTemplate),
                typeof(DataTemplate),
                typeof(ItemsControl),
                new PropertyMetadata(null, OnItemTemplateChanged));

        /// <summary>
        /// Invoked when the value of the ItemTemplate property changes.
        /// </summary>
        private static void OnItemTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ItemsControl itemsControl = (ItemsControl)d;
            if (itemsControl.ItemsHost != null)
            {
                itemsControl.Refresh(true);
            }
        }

        /// <summary>
        /// Gets or sets a path to a value on the source object to serve as the visual
        /// representation of the object.
        /// </summary>
        public string DisplayMemberPath
        {
            get { return (string)GetValue(DisplayMemberPathProperty); }
            set { SetValue(DisplayMemberPathProperty, value); }
        }

        /// <summary>
        /// Identifies the DisplayMemberPath dependency property.
        /// </summary>
        public static readonly DependencyProperty DisplayMemberPathProperty =
            DependencyProperty.Register(
                nameof(DisplayMemberPath),
                typeof(string),
                typeof(ItemsControl),
                new PropertyMetadata(string.Empty, OnDisplayMemberPathChanged));

        private static void OnDisplayMemberPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ItemsControl itemsControl = (ItemsControl)d;
            if (itemsControl.ItemsHost != null)
            {
                itemsControl.Refresh(true);
            }
        }

        /// <summary>
        /// Gets or sets the style that is used when rendering the item containers.
        /// </summary>
        public Style ItemContainerStyle
        {
            get { return (Style)GetValue(ItemContainerStyleProperty); }
            set { SetValue(ItemContainerStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the ItemContainerStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemContainerStyleProperty =
            DependencyProperty.Register(
                nameof(ItemContainerStyle),
                typeof(Style),
                typeof(ItemsControl),
                new PropertyMetadata(null, OnItemContainerStyleChanged));

        private static void OnItemContainerStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ItemsControl itemsControl = (ItemsControl)d;
            if (itemsControl.ItemsHost != null)
            {
                itemsControl.Refresh(true);
            }
        }

        #endregion Dependency Properties

        #region IGeneratorHost

        /// <summary>
        /// The view of the data
        /// </summary>
        IList IGeneratorHost.View
        {
            get { return Items; }
        }

        void IGeneratorHost.ClearContainerForItem(DependencyObject container, object item)
        {
            ClearContainerForItemOverride(container, item);
        }

        DependencyObject IGeneratorHost.GetContainerForItem(object item, DependencyObject recycledContainer)
        {
            DependencyObject container;

            // use the item directly, if possible
            if (IsItemItsOwnContainerOverride(item))
            {
                // Note: There was once an exception thrown here if 
                // (this.Items.IsUsingItemsSource && this.ItemTemplate != null) 
                // was true, stating that "ItemsControl.Items must not be a 
                // UIElement type when an ItemTemplate is set."
                // I checked in a WPF Project and it seems the ItemTemplate is 
                // simply ignored for items that are UIElements so I removed that 
                // exception.
                // todo: see if the exception of the note above should be there in 
                // certain cases (it was added in Commit 8eff80c0).
                container = item as DependencyObject;
            }
            else
            {
                container = recycledContainer == null ? GetContainerForItemOverride() : recycledContainer;
            }

            return container;
        }

        bool IGeneratorHost.IsHostForItemContainer(DependencyObject container)
        {
            // If ItemsControlFromItemContainer can determine who owns the element,
            // use its decision.
            ItemsControl ic = ItemsControlFromItemContainer(container);
            if (ic != null)
                return (ic == this);

            // If the element is in my items view, and if it can be its own ItemContainer,
            // it's mine.  Contains may be expensive, so we avoid calling it in cases
            // where we already know the answer - namely when the element has a
            // logical parent (ItemsControlFromItemContainer handles this case).  This
            // leaves only those cases where the element belongs to my items
            // without having a logical parent (e.g. via ItemsSource) and without
            // having been generated yet. HasItem indicates if anything has been generated.

            DependencyObject parent = (container as FrameworkElement)?.Parent;
            if (parent == null)
            {
                return IsItemItsOwnContainerOverride(container) &&
                    HasItems && Items.Contains(container);
            }

            // Otherwise it's not mine
            return false;
        }

        /// <summary>
        /// Return true if the item is (or is eligible to be) its own ItemContainer
        /// </summary>
        bool IGeneratorHost.IsItemItsOwnContainer(object item)
        {
            return IsItemItsOwnContainer(item);
        }

        void IGeneratorHost.PrepareItemContainer(DependencyObject container, object item)
        {
            if (ShouldApplyItemContainerStyle(container, item))
            {
                // apply the ItemContainer style (if any)
                ApplyItemContainerStyle(container, item);
            }

            // forward ItemTemplate, et al.
            PrepareContainerForItemOverride(container, item);
        }

        #endregion IGeneratorHost

        #region Public Methods

        #endregion Public Methods

        #region Protected Methods

        protected virtual void ManageCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            this.HandleItemsChanged(e);
        }

        protected virtual void UpdateItemsPanel(ItemsPanelTemplate newTemplate)
        {
            if (this.ItemsPresenter != null)
            {
                this.ItemsPresenter.Template = newTemplate;
            }
        }

        protected virtual void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            this.OnItemsSourceChanged_BeforeVisualUpdate(oldValue, newValue);
        }

        protected virtual void OnItemsSourceChanged_BeforeVisualUpdate(IEnumerable oldValue, IEnumerable newValue)
        {
            //we do nothing here
        }

        /// <summary>
        ///  Called when the value of the <see cref="ItemsControl.Items"/> property changes.
        /// </summary>
        /// <param name="e">
        /// A <see cref="NotifyCollectionChangedEventArgs"/> that contains the event data
        /// </param>
        protected virtual void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
        }

        protected internal override void INTERNAL_OnAttachedToVisualTree()
        {
            // We update the ItemsPanel only if there is no ControlTemplate.
            base.INTERNAL_OnAttachedToVisualTree();

            if (!this.HasTemplate && this.TemplateChild == null)
            {
                // If we have no template we have to create an ItemPresenter
                // manually and attach it to this control.
                // This can happen for instance if a class derive from
                // ItemsControl and specify a DefaultStyleKey and the associated
                // default style does not contain a Setter for the Template
                // property.                
                // We need to set this ItemsPresenter so that if we move from 
                // no template to a template, the "manually generated" template
                // will be detached as expected.
                // Note: this is a Silverlight specific behavior.
                // In WPF the content of the ItemsControl would simply not be
                // displayed in this scenario.
                this.TemplateChild = new ItemsPresenter();
            }
        }

        #endregion Protected Methods

        #region Internal Properties

        /// <summary>
        /// Returns enumerator to logical children
        /// </summary>
        /*protected*/
        internal override IEnumerator LogicalChildren
        {
            get
            {
                if (!HasItems)
                {
                    return EmptyEnumerator.Instance;
                }

                // Items in direct-mode of ItemCollection are the only model children.
                // note: the enumerator walks the ItemCollection.InnerList as-is,
                // no flattening of any content on model children level!
                return this.Items.LogicalChildren;
            }
        }

        internal ItemsPresenter ItemsPresenter
        {
            get { return this._itemsPresenter; }
            set { this._itemsPresenter = value; }
        }

        internal Panel ItemsHost
        {
            get
            {
                if (this.ItemsPresenter != null)
                {
                    return this.ItemsPresenter.ItemsHost;
                }
                return null;
            }
        }

        internal bool HasItems
        {
            get { return this._items != null && this._items.CountInternal > 0; }
        }

        #endregion Internal Properties

        #region Internal Methods

        // A version of Object.Equals with paranoia for mismatched types, to avoid problems
        // with classes that implement Object.Equals poorly, as in Dev11 439664, 746174, DDVSO 602650
        internal static bool EqualsEx(object o1, object o2)
        {
            try
            {
                return Object.Equals(o1, o2);
            }
            catch (InvalidCastException)
            {
                // A common programming error: the type of o1 overrides Equals(object o2)
                // but mistakenly assumes that o2 has the same type as o1:
                //     MyType x = (MyType)o2;
                // This throws InvalidCastException when o2 is a sentinel object,
                // e.g. UnsetValue, DisconnectedItem, NewItemPlaceholder, etc.
                // Rather than crash, just return false - the objects are clearly unequal.
                return false;
            }
        }

        /// <summary>
        /// Determine whether the ItemContainerStyle/StyleSelector should apply to the container
        /// </summary>
        /// <returns>true if the ItemContainerStyle should apply to the item</returns>
        internal virtual bool ShouldApplyItemContainerStyle(DependencyObject container, object item)
        {
            return true;
        }

        private void ApplyItemContainerStyle(DependencyObject container, object item)
        {
            FrameworkElement feContainer = container as FrameworkElement;

            // Control's ItemContainerStyle has first stab
            Style style = ItemContainerStyle;

            // apply the style, if found
            if (style != null)
            {
                // verify style is appropriate before applying it
                if (!style.TargetType.IsInstanceOfType(container))
                    throw new InvalidOperationException(string.Format("A style intended for type '{0}' cannot be applied to type '{1}'.", style.TargetType.Name, container.GetType().Name));

                feContainer.Style = style;
                //feContainer.IsStyleSetFromGenerator = true;
            }
        }

        /// <summary>
        /// Return true if the item is (or should be) its own item container
        /// </summary>
        internal bool IsItemItsOwnContainer(object item)
        {
            return IsItemItsOwnContainerOverride(item);
        }

        private void OnItemAdded(object item, int position)
        {
            IGeneratorHost host = this;
            DependencyObject container = host.GetContainerForItem(item, null);

            this.ItemContainerGenerator.INTERNAL_RegisterContainer(container, item);
            if (container != item)
            {
                container.SetValue(DataContextProperty, item);
            }

            this.ItemsHost.Children.Insert(position, (UIElement)container);

            host.PrepareItemContainer(container, item);
        }

        private void OnItemRemoved(object item, int position)
        {
            IGeneratorHost host = this;
            DependencyObject container = this.ItemsHost.Children[position];

            this.ItemContainerGenerator.INTERNAL_TryUnregisterContainer(container, item);
            this.ItemsHost.Children.RemoveAt(position);

            host.ClearContainerForItem(container, item);
        }

        internal void Refresh(bool reuseContainers)
        {
            IGeneratorHost host = this;
            DependencyObject[] oldContainers = new DependencyObject[Math.Max(this.ItemsHost.Children.Count, this.Items.CountInternal)];

            // First we need to get the containers and their associated item,
            // or they will be lost before being able to clear them.
            int containersCount = this.ItemsHost.Children.Count;
            object[] oldItems = new object[containersCount];
            for (int i = 0; i < containersCount; ++i)
            {
                DependencyObject container = this.ItemsHost.InternalChildren[i];
                oldContainers[i] = container;
                oldItems[i] = this.ItemContainerGenerator.ItemFromContainer(container);
            }

            if (!reuseContainers)
            {
                this.ItemsHost.InternalChildren.Clear();
                this.ItemContainerGenerator.INTERNAL_Clear();

                for (int i = 0; i < containersCount; ++i)
                {
                    host.ClearContainerForItem(oldContainers[i], oldItems[i]);
                }
            }

            int count = this.Items.CountInternal;
            for (int i = 0; i < count; ++i)
            {
                object item = this.Items[i];
                DependencyObject recycledContainer = reuseContainers ? oldContainers[i] : null;
                DependencyObject container = host.GetContainerForItem(item, recycledContainer);

                if (!reuseContainers)
                {
                    this.ItemContainerGenerator.INTERNAL_RegisterContainer(container, item);
                    if (container != item)
                    {
                        container.SetValue(FrameworkElement.DataContextProperty, item);
                    }
                    this.ItemsHost.Children.Add((UIElement)container);
                }

                host.PrepareItemContainer(container, item);
            }
        }

        internal void HandleItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            if (this.ItemsHost == null)
            {
                return;
            }

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                this.Refresh(false);
            }
            else if (e.Action == NotifyCollectionChangedAction.Add)
            {
                this.OnItemAdded(e.NewItems[0], e.NewStartingIndex);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                this.OnItemRemoved(e.OldItems[0], e.OldStartingIndex);
            }
            else if (e.Action == NotifyCollectionChangedAction.Replace)
            {
                this.OnItemAdded(e.NewItems[0], e.NewStartingIndex);
                this.OnItemRemoved(e.OldItems[0], e.OldStartingIndex + 1);
            }
            else
            {
                throw new NotSupportedException(string.Format("Unexpected collection change action '{0}'.", e.Action));
            }
        }

        private void OnItemCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.ManageCollectionChanged(e);

            this.OnItemsChanged(e);
        }

        #endregion Internal Methods

        /// <summary>
        /// Undoes the effects of the PrepareContainerForItemOverride method.
        /// </summary>
        /// <param name="element">The container element.</param>
        /// <param name="item">The item.</param>
        protected virtual void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            // Note: This is the WPF implementation.
            // Silverlight does not clear containers on the ItemsControl level.
            //ContentControl cc;
            //ContentPresenter cp;

            //if ((cc = element as ContentControl) != null)
            //{
            //    cc.ClearContentControl(item);
            //}
            //else if ((cp = element as ContentPresenter) != null)
            //{
            //    cp.ClearContentPresenter(item);
            //}
        }

        /// <summary>
        /// Creates or identifies the element that is used to display the given item.
        /// </summary>
        /// <returns>
        /// The element that is used to display the given item.
        /// </returns>
        protected virtual DependencyObject GetContainerForItemOverride()
        {
            return new ContentPresenter();
        }

        /// <summary>
        /// Determines whether the specified item is (or is eligible to be) its 
        /// own container.
        /// </summary>
        /// <param name="item">
        /// The item to check.
        /// </param>
        /// <returns>
        /// True if the item is (or is eligible to be) its own container; otherwise,
        /// false.
        /// </returns>
        protected virtual bool IsItemItsOwnContainerOverride(object item)
        {
            return (item is UIElement);
        }

        /// <summary>
        /// Prepares the specified element to display the specified item.
        /// </summary>
        /// <param name="element">
        /// The element that's used to display the specified item.
        /// </param>
        /// <param name="item">
        /// The item to display.
        /// </param>
        protected virtual void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            ContentControl cc;
            ContentPresenter cp;

            if (this.ItemTemplate != null && !string.IsNullOrWhiteSpace(this.DisplayMemberPath))
            {
                throw new InvalidOperationException("Cannot set both DisplayMemberPath and ItemTemplate.");
            }

            DataTemplate template = null;
            if (!(item is UIElement))
            {
                template = this.ItemTemplate;
                if (template == null)
                {
                    template = GetDataTemplateForDisplayMemberPath(this.DisplayMemberPath);
                }
            }

            if ((cc = element as ContentControl) != null)
            {
                cc.PrepareContentControl(item, template);
            }
            else if ((cp = element as ContentPresenter) != null)
            {
                cp.PrepareContentPresenter(item, template);
            }
        }

        internal static DataTemplate GetDataTemplateForDisplayMemberPath(string displayMemberPath)
        {
            DataTemplate template = new DataTemplate();

            template._methodToInstantiateFrameworkTemplate = control =>
            {
                TemplateInstance templateInstance = new TemplateInstance();

                TextBlock textBlock = new TextBlock();
                textBlock.SetBinding(TextBlock.TextProperty, new Binding(displayMemberPath ?? string.Empty));

                templateInstance.TemplateContent = textBlock;

                return templateInstance;
            };

            return template;
        }

        public static ItemsControl GetItemsOwner(DependencyObject element)
        {
            ItemsControl container = null;
            Panel panel = element as Panel;

            if (panel != null && panel.IsItemsHost)
            {
                // see if element was generated for an ItemsPresenter
                ItemsPresenter ip = ItemsPresenter.FromPanel(panel);

                if (ip != null)
                {
                    // if so use the element whose style begat the ItemsPresenter
                    container = ip.Owner;
                }
                //else
                //{
                //    // otherwise use element's templated parent
                //    container = panel.TemplatedParent as ItemsControl;
                //}
            }

            return container;
        }

        public static ItemsControl ItemsControlFromItemContainer(DependencyObject container)
        {
            UIElement ui = container as UIElement;
            if (ui == null)
                return null;

            // ui appeared in items collection
            ItemsControl ic = (ui as FrameworkElement)?.Parent as ItemsControl;
            if (ic != null)
            {
                // this is the right ItemsControl as long as the item
                // is (or is eligible to be) its own container
                IGeneratorHost host = ic as IGeneratorHost;
                if (host.IsItemItsOwnContainer(ui))
                    return ic;
                else
                    return null;
            }

            ui = VisualTreeHelper.GetParent(ui) as UIElement;

            return ItemsControl.GetItemsOwner(ui);
        }

        #region Obsolete

        /// <summary>
        /// Derived classes can call this methed in their constructor if they 
        /// want to disable the default rendering of the ItemsControl. It can 
        /// be useful for example to replace the rendering with a custom 
        /// HTML-based one.
        /// </summary>
        [Obsolete("Disabling default rendering is not supported anymore.")]
        protected void DisableDefaultRendering()
        {
        }

        /// <summary>
        /// Returns the item itself if the item is already a container of the 
        /// correct type, otherwise it returns null if no container is to be 
        /// created, or it returns the new container otherwise.
        /// </summary>
        /// <param name="item">The item to generate the container with.</param>
        /// <returns>
        /// Returns the item itself if the item is already a container of the 
        /// correct type, otherwise it returns null if no container is to be 
        /// created, or it returns the new container otherwise.
        /// </returns>
        [Obsolete]
        protected virtual SelectorItem INTERNAL_GenerateContainer(object item)
        {
            return (SelectorItem)this.GetContainerFromItem(item);
        }

        [Obsolete]
        protected virtual void OnChildItemRemoved(object item)
        {
            // This is intented to be overridden by the controls that have 
            // a "SelectedItem" to make sure that the item is de-selected 
            // in case that the element is removed.
        }

        [Obsolete]
        protected FrameworkElement GenerateFrameworkElementToRenderTheItem(object item)
        {
            //---------------------------------------------------
            // if the item is a FrameworkElement, return itself
            //---------------------------------------------------
            FrameworkElement result = item as FrameworkElement;
            if (result == null)
            {
                object displayElement = PropertyPathHelper.AccessValueByApplyingPropertyPathIfAny(item, this.DisplayMemberPath);
                if (this.ItemTemplate != null)
                {
                    //---------------------------------------------------
                    // An ItemTemplate was specified, so we instantiate 
                    // it and return it
                    //---------------------------------------------------

                    // Apply the data template
                    result = ItemTemplate.INTERNAL_InstantiateFrameworkTemplate();
                    result.DataContext = displayElement;
                }
                else
                {
                    //---------------------------------------------------
                    // Otherwise we simply call "ToString()" to display 
                    // the item as a string inside a TextBlock
                    //---------------------------------------------------

                    ContentPresenter container = new ContentPresenter();
                    Binding b = new Binding(this.DisplayMemberPath);
                    container.SetBinding(ContentPresenter.ContentProperty, b);
                    container.DataContext = item;
                    result = container;
                }
            }
#if WORKINPROGRESS
            this.PrepareContainerForItemOverride(result, item);
#endif
            return result;
        }

        /// <summary>
        /// Create or identify the element used to display the given item.
        /// </summary>
        /// <returns>
        /// The element that is used to display the given item.
        /// </returns>
        [Obsolete]
        protected virtual DependencyObject GetContainerFromItem(object item)
        {
            return null;
        }

        #endregion Obsolete
    }
}