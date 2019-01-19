using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Criterion;
using NHibernate.CustomQueries.Data;
using NHibernate.SqlTypes;
using NHibernate.Type;

namespace NHibernate.CustomQueries
{
    public static class QueryExtensions
    {
        public static IQueryOver<TRoot, TSubType> WhereIn<TRoot, TSubType>(this IQueryOver<TRoot, TSubType> query, Expression<Func<TSubType, object>> expression, string tableName)
        {
            if (!tableName.StartsWith("#"))
            {
                tableName = "#" + tableName;
            }

            query.UnderlyingCriteria.Add(new JoinTemporaryTableExpression(Projections.Property<TSubType>(expression), tableName, "Id"));
            return query;
        }

        public static ISession CreateTemporaryIdTable<T>(this ISession session, string tableName, T[] values)
        {
            if (!tableName.StartsWith("#"))
            {
                tableName = "#" + tableName;
            }

            SqlType type;
            try
            {
                type = ((NullableType) NHibernateUtil.GuessType(values.First())).SqlType;
            }
            catch (Exception exception)
            {
                throw new ArgumentException($"Unsupported type \"{values.First().GetType().FullName}\" for values argument");
            }

            var sqlType = session.GetSessionImplementation().Factory.Dialect.GetTypeName(type);
            session.CreateSQLQuery($"CREATE TABLE {tableName} (Id {sqlType} NOT NULL PRIMARY KEY);").ExecuteUpdate();

            foreach (var value in values)
            {
                session.CreateSQLQuery($"INSERT INTO {tableName} (Id) VALUES (:Id);").SetParameter("Id", value).ExecuteUpdate();
            }

            return session;
        }
    }
}
