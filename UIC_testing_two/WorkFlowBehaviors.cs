using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace UIC_testing_two
{
    public class WorkFlowBehaviors : Behavior<Button>
    {


        public bool NeedsSave
        {
            get { return (bool)GetValue(NeedsSaveProperty); }
            set { SetValue(NeedsSaveProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NeedsSave.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NeedsSaveProperty =
            DependencyProperty.Register("NeedsSave", typeof(bool), typeof(WorkFlowBehaviors), new PropertyMetadata(false, OnNeedsSaveChanged));

        private static void OnNeedsSaveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = ((WorkFlowBehaviors)d);
            bool needsSave = ((bool)e.NewValue);
            if (needsSave)
                behavior.AssociatedObject.Visibility = Visibility.Visible;
            else
                behavior.AssociatedObject.Visibility = Visibility.Collapsed;

        }



    }
}
