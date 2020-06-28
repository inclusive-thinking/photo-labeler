using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using PhotoLabeler.Entities;
using PhotoLabeler.Components.Extensions;
using Microsoft.JSInterop;

namespace PhotoLabeler.Components
{

    public partial class TreeView
    {
        private Entities.TreeView<Photo> model;
        private Guid currentModelUniqueId;

        [Inject]
        IJSRuntime  jsRuntime {get; set; }


        [Parameter]
        public Entities.TreeView<Photo> Model
        {
            get => model;
            set
            {
                if (currentModelUniqueId != value.UniqueId)
                {
                    executeONSelectAfterRender = true;
                    currentModelUniqueId = value.UniqueId;
                }
                model = value;
            }
        }

        [Parameter]
        public string Id { get; set; }

        [Parameter]
        public EventCallback<TreeViewItem<Photo>> OnItemSelected { get; set; }

        private bool executeONSelectAfterRender = false;

        private RenderFragment fragment;

        private bool focusOnItemAfterRender = false;

        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();
            if (string.IsNullOrWhiteSpace(this.Id))
            {
                this.Id = Guid.NewGuid().ToString();
            }
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            this.fragment = (builder) =>
            {
                builder.OpenElement(1, "ul");
                builder.AddAttribute(1, "class", "tree");
                builder.AddAttribute(2, "role", "tree");
                if (!string.IsNullOrWhiteSpace(this.Id))
                {
                    builder.AddAttribute(3, "id", this.Id);
                }
                for (var i = 0; i < Model.Items.Count; i++)
                {
                    var item = Model.Items[i];
                    BuildItem(builder, item, i + 1);
                }
                builder.CloseElement();
            };
        }

        private void BuildItem(RenderTreeBuilder builder, TreeViewItem<Photo> item, int elementNumber)
        {
            var aAttributeNumber = 1;
            builder.OpenElement(elementNumber, "li");
            builder.AddAttribute(1, "data-value", System.Web.HttpUtility.UrlEncode(item.Path));
            builder.AddAttribute(2, "role", "presentation");
            if (item.Children != null && item.Children.Any())
            {
                builder.AddAttribute(3, "class", "hasChildren");
            }
            builder.OpenElement(1, "a");
            builder.AddAttribute(aAttributeNumber++, "href", "#");
            builder.AddAttribute(aAttributeNumber++, "role", "treeitem");
            if (item.Selected)
            {
                builder.AddAttribute(aAttributeNumber++, "aria-selected", "true");
                builder.AddAttribute(aAttributeNumber++, "tabindex", "0");
            }
            else
            {
                builder.AddAttribute(aAttributeNumber++, "tabindex", "-1");
            }
            builder.AddAttribute(aAttributeNumber++, "aria-label", item.Name);
            builder.AddAttribute(aAttributeNumber++, "onkeydown", EventCallback.Factory.Create(this, async (KeyboardEventArgs e) =>
            {
                await Item_KeyDown(item, e);
            }));
            builder.AddAttribute(aAttributeNumber++, "onclick", EventCallback.Factory.Create(this, async () =>
            {
                await Item_Click(item);
            }));
            if (item.Children != null && item.Children.Any())
            {
                builder.AddAttribute(aAttributeNumber++, "aria-expanded", item.Expanded ? "true" : "false");
            }
            builder.AddContent(1, item.Name);
            builder.CloseElement();

            if (item.Expanded && item.Children != null && item.Children.Any())
            {
                builder.OpenElement(2, "ul");
                builder.AddAttribute(1, "class", "children");
                builder.AddAttribute(2, "role", "group");
                for (var i = 0; i < item.Children.Count(); i++)
                {
                    BuildItem(builder, item.Children[i], i + 1);
                }
                builder.CloseElement();
            }
            builder.CloseElement();
        }

        private async Task Item_KeyDown(TreeViewItem<Photo> item, KeyboardEventArgs e)
        {
            TreeViewItem<Photo>
            newSelectedItem = null;
            var flatItems = item.TreeView.FlatItems.Value;
            focusOnItemAfterRender = false;

            switch (e.Key)
            {
                case "ArrowUp":
                    if (item.ItemIndex == 0)
                    {
                        if (item.Level == 0)
                        {
                            return;
                        }
                        newSelectedItem = item.Parent;
                        break;
                    }
                    var previousSibling = item.Parent != null ? item.Parent.Children[item.ItemIndex - 1] : item.TreeView.Items[item.ItemIndex - 1];
                    if (!previousSibling.Expanded)
                    {
                        newSelectedItem = previousSibling;
                        break;
                    }
                    var lastItem = previousSibling.Children.Last();
                    while (lastItem.Expanded)
                    {
                        lastItem = lastItem.Children.Last();
                    }
                    newSelectedItem = lastItem;
                    break;
                case "ArrowDown":
                    var nextItem = item;
                    if (nextItem.Expanded && nextItem.Children != null && nextItem.Children.Any())
                    {
                        newSelectedItem = nextItem.Children.First();
                        break;
                    }
                    while (nextItem != null && nextItem.IsLast())
                    {
                        nextItem = nextItem.Parent;
                    }
                    if (nextItem == null)
                    {
                        return;
                    }
                    if (nextItem.Parent != null)
                    {
                        nextItem = nextItem.Parent.Children[nextItem.ItemIndex + 1];
                    }
                    else
                    {
                        nextItem = nextItem.TreeView.Items[nextItem.ItemIndex + 1];
                    }
                    newSelectedItem = nextItem;
                    break;
                case "ArrowLeft":
                    if (item.Parent == null && !item.Expanded)
                    {
                        return;
                    }
                    if (item.Expanded)
                    {
                        item.Expanded = false;
                        return;
                    }
                    newSelectedItem = item.Parent;
                    break;
                case "ArrowRight":
                    if (item.Children == null || !item.Children.Any())
                    {
                        return;
                    }
                    if (!item.Expanded)
                    {
                        item.Expanded = true;
                        return;
                    }
                    newSelectedItem = item.Children.First();
                    break;
                case "Home":
                    newSelectedItem = item.TreeView.Items.First();
                    break;
                case "End":
                    for (int i = flatItems.Count - 1; i >= 0; i--)
                    {
                        if (!flatItems[i].IsVisible())
                        {
                            continue;
                        }
                        newSelectedItem = flatItems[i];
                        break;
                    }
                    break;
                case "PageDown":
                    var itemIndexInFlatItems = flatItems.IndexOf(item);
                    var skipedItems = 0;
                    var nextItemDown = item;

                    for (var i = itemIndexInFlatItems + 1; i < flatItems.Count && skipedItems < 10; i++)
                    {
                        if (flatItems[i].IsVisible())
                        {
                            skipedItems++;
                            nextItemDown = flatItems[i];
                        }
                    }
                    if (nextItemDown != item)
                    {
                        newSelectedItem = nextItemDown;
                    }
                    break;
                default:
                    return;
            }
            if (newSelectedItem != null)
            {
                item.Selected = false;
                newSelectedItem.Selected = true;
                Model.SelectedItem = newSelectedItem;
                focusOnItemAfterRender = true;
                if (OnItemSelected.HasDelegate)
                {
                    await OnItemSelected.InvokeAsync(Model.SelectedItem);
                }
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (focusOnItemAfterRender)
            {
                await jsRuntime.InvokeVoidAsync("jsInteropFunctions.focusSelectedItemInsideContainer", this.Id);
            }
            if (executeONSelectAfterRender && OnItemSelected.HasDelegate && Model.SelectedItem != null)
            {
                executeONSelectAfterRender = false;
                await OnItemSelected.InvokeAsync(Model.SelectedItem);
            }
        }

        private async Task Item_Click(TreeViewItem<Photo> item)
        {
            var previousItem = item.TreeView.FlatItems.Value.SingleOrDefault(i => i.Selected);
            if (previousItem == item)
            {
                return;
            }
            item.Selected = true;
            previousItem.Selected = false;
            Model.SelectedItem = item;
            focusOnItemAfterRender = true;
            if (OnItemSelected.HasDelegate)
            {
                await OnItemSelected.InvokeAsync(Model.SelectedItem);
            }
        }
    }
}