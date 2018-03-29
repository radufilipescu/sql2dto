using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.Core
{
    public class SqlCaseWhenExpression : SqlExpression
    {
        private SqlCaseWhenExpression()
        {
            _whenThenExpressions = new List<(SqlExpression, SqlExpression)>();
        }

        internal SqlCaseWhenExpression(SqlExpression onExpression = null)
            : this()
        {
            _onExpression = onExpression;
        }

        private SqlExpression _onExpression;
        public SqlExpression GetOnExpression() => _onExpression;

        private List<(SqlExpression, SqlExpression)> _whenThenExpressions;
        public List<(SqlExpression, SqlExpression)> GetWhenThenExpressions() => _whenThenExpressions;

        private SqlExpression _elseExpression;
        public SqlExpression GetElseExpression() => _elseExpression;

        public override SqlExpressionType GetExpressionType() => SqlExpressionType.CASE_WHEN;

        #region WHEN
        #region (expr, ...)
        public SqlCaseWhenExpression When(SqlExpression whenExpression, SqlExpression thenExpression)
        {
            _whenThenExpressions.Add((whenExpression, thenExpression));
            return this;
        }

        public SqlCaseWhenExpression When(SqlExpression whenExpression, int thenConstant)
        {
            _whenThenExpressions.Add((whenExpression, Sql.Const(thenConstant)));
            return this;
        }

        public SqlCaseWhenExpression When(SqlExpression whenExpression, double thenConstant)
        {
            _whenThenExpressions.Add((whenExpression, Sql.Const(thenConstant)));
            return this;
        }

        public SqlCaseWhenExpression When(SqlExpression whenExpression, float thenConstant)
        {
            _whenThenExpressions.Add((whenExpression, Sql.Const(thenConstant)));
            return this;
        }

        public SqlCaseWhenExpression When(SqlExpression whenExpression, decimal thenConstant)
        {
            _whenThenExpressions.Add((whenExpression, Sql.Const(thenConstant)));
            return this;
        }

        public SqlCaseWhenExpression When(SqlExpression whenExpression, string thenConstant)
        {
            _whenThenExpressions.Add((whenExpression, Sql.Const(thenConstant)));
            return this;
        }
        #endregion

        #region (int, ...)
        public SqlCaseWhenExpression When(int whenConstant, SqlExpression thenExpression)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), thenExpression));
            return this;
        }

        public SqlCaseWhenExpression When(int whenConstant, int thenConstant)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), Sql.Const(thenConstant)));
            return this;
        }

        public SqlCaseWhenExpression When(int whenConstant, double thenConstant)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), Sql.Const(thenConstant)));
            return this;
        }

        public SqlCaseWhenExpression When(int whenConstant, float thenConstant)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), Sql.Const(thenConstant)));
            return this;
        }

        public SqlCaseWhenExpression When(int whenConstant, decimal thenConstant)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), Sql.Const(thenConstant)));
            return this;
        }

        public SqlCaseWhenExpression When(int whenConstant, string thenConstant)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), Sql.Const(thenConstant)));
            return this;
        }
        #endregion

        #region (double, ...)
        public SqlCaseWhenExpression When(double whenConstant, SqlExpression thenExpression)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), thenExpression));
            return this;
        }

        public SqlCaseWhenExpression When(double whenConstant, int thenConstant)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), Sql.Const(thenConstant)));
            return this;
        }

        public SqlCaseWhenExpression When(double whenConstant, double thenConstant)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), Sql.Const(thenConstant)));
            return this;
        }

        public SqlCaseWhenExpression When(double whenConstant, float thenConstant)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), Sql.Const(thenConstant)));
            return this;
        }

        public SqlCaseWhenExpression When(double whenConstant, decimal thenConstant)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), Sql.Const(thenConstant)));
            return this;
        }

        public SqlCaseWhenExpression When(double whenConstant, string thenConstant)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), Sql.Const(thenConstant)));
            return this;
        }
        #endregion

        #region (float, ...)
        public SqlCaseWhenExpression When(float whenConstant, SqlExpression thenExpression)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), thenExpression));
            return this;
        }

        public SqlCaseWhenExpression When(float whenConstant, int thenConstant)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), Sql.Const(thenConstant)));
            return this;
        }

        public SqlCaseWhenExpression When(float whenConstant, double thenConstant)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), Sql.Const(thenConstant)));
            return this;
        }

        public SqlCaseWhenExpression When(float whenConstant, float thenConstant)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), Sql.Const(thenConstant)));
            return this;
        }

        public SqlCaseWhenExpression When(float whenConstant, decimal thenConstant)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), Sql.Const(thenConstant)));
            return this;
        }

        public SqlCaseWhenExpression When(float whenConstant, string thenConstant)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), Sql.Const(thenConstant)));
            return this;
        }
        #endregion

        #region (decimal, ...)
        public SqlCaseWhenExpression When(decimal whenConstant, SqlExpression thenExpression)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), thenExpression));
            return this;
        }

        public SqlCaseWhenExpression When(decimal whenConstant, int thenConstant)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), Sql.Const(thenConstant)));
            return this;
        }

        public SqlCaseWhenExpression When(decimal whenConstant, double thenConstant)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), Sql.Const(thenConstant)));
            return this;
        }

        public SqlCaseWhenExpression When(decimal whenConstant, float thenConstant)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), Sql.Const(thenConstant)));
            return this;
        }

        public SqlCaseWhenExpression When(decimal whenConstant, decimal thenConstant)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), Sql.Const(thenConstant)));
            return this;
        }

        public SqlCaseWhenExpression When(decimal whenConstant, string thenConstant)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), Sql.Const(thenConstant)));
            return this;
        }
        #endregion

        #region (string, ...)
        public SqlCaseWhenExpression When(string whenConstant, SqlExpression thenExpression)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), thenExpression));
            return this;
        }

        public SqlCaseWhenExpression When(string whenConstant, int thenConstant)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), Sql.Const(thenConstant)));
            return this;
        }

        public SqlCaseWhenExpression When(string whenConstant, double thenConstant)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), Sql.Const(thenConstant)));
            return this;
        }

        public SqlCaseWhenExpression When(string whenConstant, float thenConstant)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), Sql.Const(thenConstant)));
            return this;
        }

        public SqlCaseWhenExpression When(string whenConstant, decimal thenConstant)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), Sql.Const(thenConstant)));
            return this;
        }

        public SqlCaseWhenExpression When(string whenConstant, string thenConstant)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), Sql.Const(thenConstant)));
            return this;
        }
        #endregion
        #endregion

        #region ELSE
        public SqlCaseWhenExpression Else(SqlExpression elseExpression)
        {
            _elseExpression = elseExpression;
            return this;
        }

        public SqlCaseWhenExpression Else(int elseConstant)
        {
            _elseExpression = Sql.Const(elseConstant);
            return this;
        }

        public SqlCaseWhenExpression Else(double elseConstant)
        {
            _elseExpression = Sql.Const(elseConstant);
            return this;
        }

        public SqlCaseWhenExpression Else(float elseConstant)
        {
            _elseExpression = Sql.Const(elseConstant);
            return this;
        }

        public SqlCaseWhenExpression Else(decimal elseConstant)
        {
            _elseExpression = Sql.Const(elseConstant);
            return this;
        }

        public SqlCaseWhenExpression Else(string elseConstant)
        {
            _elseExpression = Sql.Const(elseConstant);
            return this;
        }
        #endregion

        public SqlCaseWhenExpression End()
        {
            return this;
        }
    }
}
