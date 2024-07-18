﻿using System.Collections.Generic;
using RogueElements;
using System.Linq;

namespace RogueEssence.Menu
{
    public abstract class ChoiceMenu : InteractableMenu
    {
        public override List<IMenuElement> Elements {
            get
            {
                return NonChoices.Concat(Choices).ToList();
            }
            protected set
            {
                //Realistically only affects initialization
                NonChoices = value;
                Choices = new List<IChoosable>();
            }
        }
        public List<IMenuElement> NonChoices;
        public List<IChoosable> Choices;

        protected MenuCursor cursor;

        public ChoiceMenu()
        {
            cursor = new MenuCursor(MenuLabel.CURSOR, this, Dir4.Right);
            NonChoices = new List<IMenuElement> {cursor};
            Choices = new List<IChoosable>();
        }

        public override Dictionary<string, int> GetElementIndexesByLabel(params string[] labels)
            => SearchLabels(labels, Elements);

        public int GetChoiceIndexByLabel(string label)
        {
            if (GetChoiceIndexesByLabel(label).TryGetValue(label, out int ret)) return ret;
            return -1;
        }
        public virtual Dictionary<string, int> GetChoiceIndexesByLabel(params string[] labels)
            => SearchLabels(labels, Choices);

        public int GetNonChoiceIndexByLabel(string label)
        {
            if (GetNonChoiceIndexesByLabel(label).TryGetValue(label, out int ret)) return ret;
            return -1;
        }
        public virtual Dictionary<string, int> GetNonChoiceIndexesByLabel(params string[] labels)
            => SearchLabels(labels, NonChoices);
    }
}
