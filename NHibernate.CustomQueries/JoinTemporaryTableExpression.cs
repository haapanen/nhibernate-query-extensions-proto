using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Criterion;
using NHibernate.Engine;
using NHibernate.SqlCommand;
using NHibernate.Util;

namespace NHibernate.CustomQueries
{
    public class JoinTemporaryTableExpression : AbstractCriterion
    {
        private readonly IProjection _projection;
        private readonly string _temporaryTable;
        private readonly string _temporaryTableProperty;

        public JoinTemporaryTableExpression(IProjection projection, string temporaryTable, string temporaryTableProperty)
        {
            _projection = projection;
            _temporaryTable = temporaryTable;
            _temporaryTableProperty = temporaryTableProperty;
        }

        public override string ToString()
        {
            return "Not implemented";
        }

        public override SqlString ToSqlString(ICriteria criteria, ICriteriaQuery criteriaQuery, IDictionary<string, IFilter> enabledFilters)
        {
            var result = new SqlStringBuilder();
            var columnNames = CriterionUtil.GetColumnNames(null, _projection, criteriaQuery, criteria, enabledFilters);    

            for (int columnIndex = 0; columnIndex < columnNames.Length; columnIndex++)
            {
                SqlString columnName = columnNames[columnIndex];

                if (columnIndex > 0)
                    result.Add(" and ");

                result.Add(columnName).Add(" in (").Add($"SELECT {_temporaryTableProperty} FROM {_temporaryTable}")
                    .Add(")");
            }

            return result.ToSqlString();
        }

        public override TypedValue[] GetTypedValues(ICriteria criteria, ICriteriaQuery criteriaQuery)
        {
            return _projection.GetTypedValues(criteria, criteriaQuery);
        }

        public override IProjection[] GetProjections()
        {
            return new []{_projection};
        }
    }
}
