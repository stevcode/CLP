using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catel.Data;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;

namespace CLP.Models
{


    /// <summary>
    /// MathPageDefinition : Data object class which fully supports serialization, property changed notifications,
    /// backwards compatibility and error checking.
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    public class MathPageDefinition : DataObjectBase<MathPageDefinition>
    {
        #region Fields
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new object from scratch.
        /// </summary>
        public MathPageDefinition() { }

#if !SILVERLIGHT
        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected MathPageDefinition(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
#endif
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public ObservableCollection<MathRelation> Relations
        {
            get { return GetValue<ObservableCollection<MathRelation>>(RelationsProperty); }
            set { SetValue(RelationsProperty, value); }
        }

        /// <summary>
        /// Register the Relations property so it is known in the class.
        /// </summary>
        public static readonly PropertyData RelationsProperty = RegisterProperty("Relations", typeof(ObservableCollection<MathRelation>), new ObservableCollection<MathRelation>());

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public ObservableCollection<String> Constraints
        {
            get { return GetValue<ObservableCollection<String>>(ConstraintsProperty); }
            set { SetValue(ConstraintsProperty, value); }
        }

        /// <summary>
        /// Register the Constraints property so it is known in the class.
        /// </summary>
        public static readonly PropertyData ConstraintsProperty = RegisterProperty("Constraints", typeof(ObservableCollection<String>), new ObservableCollection<String>());

        #endregion

        #region Methods

        #endregion
    }

}