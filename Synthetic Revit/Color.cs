using System;

using RevitColor = Autodesk.Revit.DB.Color;

using Autodesk.DesignScript.Runtime;
using DynColor = DSCore.Color;

namespace Synthetic.Revit
{
    /// <summary>
    /// A wrapper for a Revit Color object.
    /// </summary>
    public class ColorWrapper
    {
        #region Internal Properties

        internal RevitColor revitColor { get; private set; }

        internal int _red
        { get { return (int)revitColor.Red; } }

        internal int _green
        { get { return (int)revitColor.Green; } }

        internal int _blue
        { get { return (int)revitColor.Blue; } }

        internal ColorWrapper (RevitColor _revitColor)
        {
            this.revitColor = _revitColor;
        }

        #endregion

        /// <summary>
        /// Creates a wrapper for Revit color by red, green and blue components.
        /// </summary>
        /// <param name="red">Red component of the color.</param>
        /// <param name="green">Green component of the color.</param>
        /// <param name="blue">Blue component of the color.</param>
        /// <returns name="Color">A wrapped Revit color.</returns>
        public static ColorWrapper ByRGB (
            [DefaultArgument("0")] int red,
            [DefaultArgument("0")] int green,
            [DefaultArgument("0")] int blue
            )
        {
            return new ColorWrapper(new RevitColor((byte)red, (byte)green, (byte)blue));
        }

        /// <summary>
        /// Creates a wrapper for a Revit color by converting a dynamo color.
        /// </summary>
        /// <param name="dynamoColor">A color created using the Dynamo Core Color nodes.</param>
        /// <returns name="Color">A wrapped Revit color.</returns>
        public static ColorWrapper ByDynamoColor (DynColor dynamoColor)
        {
            return new ColorWrapper(new RevitColor((byte)dynamoColor.Red, (byte)dynamoColor.Green, (byte)dynamoColor.Blue));
        }

        /// <summary>
        /// Converts the Revit color into a Dynamo Core Color.
        /// </summary>
        /// <param name="color">A wrapped Revit color</param>
        /// <returns name="dynamoColor">A Dynamo Core Color.</returns>
        public static DynColor ToDynamoColor (ColorWrapper color)
        {
            return DynColor.ByARGB(255, color._red, color._green, color._blue);
        }

        /// <summary>
        /// Creates a new wrapper for a Revit color
        /// </summary>
        /// <param name="color">A Revit Color</param>
        /// <returns name="color">A wrapped Revit color.</returns>
        public static ColorWrapper Wrap(RevitColor color)
        {
            return new ColorWrapper(color);
        }

        /// <summary>
        /// Retrives the unwrapped Revit color.
        /// </summary>
        /// <param name="color">A wrapped Revit color.</param>
        /// <returns name="Revit color">A Revit color.</returns>
        public static RevitColor UnwrapColor (ColorWrapper color)
        {
            return color.revitColor;
        }

        /// <summary>
        /// A string representation of the color.
        /// </summary>
        /// <returns name="string">A string representation.</returns>
        public override string ToString()
        {
            return string.Format("Revit Color Wrapper ( Red = {0}, Green = {1}, Blue = {2} )", this._red, this._green, this._blue);
        }

        /// <summary>
        /// Get the Red component of the color.
        /// </summary>
        /// <param name="color">A wrapped Revit color.</param>
        /// <returns name="red">The red component.</returns>
        public static int Red (ColorWrapper color)
        { return color._red; }

        /// <summary>
        /// Get the Greem component of the color.
        /// </summary>
        /// <param name="color">A wrapped Revit color.</param>
        /// <returns name="green">The green component.</returns>
        public static int Green(ColorWrapper color)
        { return color._green; }

        /// <summary>
        /// Get the Blue component of the color.
        /// </summary>
        /// <param name="color">A wrapped Revit color.</param>
        /// <returns name="blue">The blue component.</returns>
        public static int Blue(ColorWrapper color)
        { return color._blue; }

    }
}
