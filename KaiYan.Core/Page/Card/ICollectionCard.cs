﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaiYan.Core.Page.Card
{
    public interface ICollectionCard
    {
        IList<ICardItem> Items { get; }
    }
}
