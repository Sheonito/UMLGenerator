using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UMLAutoGenerator;
using UnityEngine;
using UnityEngine.UIElements;

public class ViewNavigation : Singleton<ViewNavigation>
{
    private List<VisualElement> elements;
    private Stack<VisualElement> views;
    private VisualElement root;

    public ViewNavigation()
    {
        instance = this;
        elements = new List<VisualElement>();
        views = new Stack<VisualElement>();
        root = UMLGenerator.root;
    }

    public void InitElements(IEnumerable<VisualElement> children)
    {
        foreach (var element in children)
        {
            if (elements.Contains(element) == false && !string.IsNullOrEmpty(element.name) &&
                !string.IsNullOrWhiteSpace(element.name))
            {
                elements.Add(element);
            }

            if (element.childCount != 0)
            {
                var secondChildren = element.Children();
                InitElements(secondChildren);
            }
        }
    }

    public void Push(string viewName)
    {
        VisualElement nextView = elements.FirstOrDefault(element => element.name.Contains(viewName));
        if (nextView != null)
        {
            if (views.Count() != 0)
            {
                VisualElement curView = views.Peek();
                curView.style.display = DisplayStyle.None;
            }

            nextView.style.display = DisplayStyle.Flex;

            views.Push(nextView);
        }
    }

    public void Pop()
    {
        if (views.Count == 0)
            return;

        VisualElement curView = views.Peek();
        curView.style.display = DisplayStyle.None;
        views.Pop();
        VisualElement nextView = views.Peek();

        if (nextView != null)
            nextView.style.display = DisplayStyle.Flex;
    }
}