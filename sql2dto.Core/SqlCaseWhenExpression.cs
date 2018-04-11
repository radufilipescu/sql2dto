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
        public SqlCaseWhenExpression When(SqlExpression whenExpression, SqlExpression then)
        {
            _whenThenExpressions.Add((whenExpression, then));
            return this;
        }

        public SqlCaseWhenExpression When(SqlExpression whenExpression, int then)
        {
            _whenThenExpressions.Add((whenExpression, Sql.Const(then)));
            return this;
        }

        public SqlCaseWhenExpression When(SqlExpression whenExpression, double then)
        {
            _whenThenExpressions.Add((whenExpression, Sql.Const(then)));
            return this;
        }

        public SqlCaseWhenExpression When(SqlExpression whenExpression, float then)
        {
            _whenThenExpressions.Add((whenExpression, Sql.Const(then)));
            return this;
        }

        public SqlCaseWhenExpression When(SqlExpression whenExpression, decimal then)
        {
            _whenThenExpressions.Add((whenExpression, Sql.Const(then)));
            return this;
        }

        public SqlCaseWhenExpression When(SqlExpression whenExpression, string then)
        {
            _whenThenExpressions.Add((whenExpression, Sql.Const(then)));
            return this;
        }
        #endregion

        #region (int, ...)
        public SqlCaseWhenExpression When(int whenConstant, SqlExpression then)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), then));
            return this;
        }

        public SqlCaseWhenExpression When(int whenConstant, int then)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), Sql.Const(then)));
            return this;
        }

        public SqlCaseWhenExpression When(int whenConstant, double then)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), Sql.Const(then)));
            return this;
        }

        public SqlCaseWhenExpression When(int whenConstant, float then)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), Sql.Const(then)));
            return this;
        }

        public SqlCaseWhenExpression When(int whenConstant, decimal then)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), Sql.Const(then)));
            return this;
        }

        public SqlCaseWhenExpression When(int whenConstant, string then)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), Sql.Const(then)));
            return this;
        }
        #endregion

        #region (double, ...)
        public SqlCaseWhenExpression When(double whenConstant, SqlExpression then)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), then));
            return this;
        }

        public SqlCaseWhenExpression When(double whenConstant, int then)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), Sql.Const(then)));
            return this;
        }

        public SqlCaseWhenExpression When(double whenConstant, double then)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), Sql.Const(then)));
            return this;
        }

        public SqlCaseWhenExpression When(double whenConstant, float then)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), Sql.Const(then)));
            return this;
        }

        public SqlCaseWhenExpression When(double whenConstant, decimal then)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), Sql.Const(then)));
            return this;
        }

        public SqlCaseWhenExpression When(double whenConstant, string then)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), Sql.Const(then)));
            return this;
        }
        #endregion

        #region (float, ...)
        public SqlCaseWhenExpression When(float whenConstant, SqlExpression then)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), then));
            return this;
        }

        public SqlCaseWhenExpression When(float whenConstant, int then)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), Sql.Const(then)));
            return this;
        }

        public SqlCaseWhenExpression When(float whenConstant, double then)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), Sql.Const(then)));
            return this;
        }

        public SqlCaseWhenExpression When(float whenConstant, float then)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), Sql.Const(then)));
            return this;
        }

        public SqlCaseWhenExpression When(float whenConstant, decimal then)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), Sql.Const(then)));
            return this;
        }

        public SqlCaseWhenExpression When(float whenConstant, string then)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), Sql.Const(then)));
            return this;
        }
        #endregion

        #region (decimal, ...)
        public SqlCaseWhenExpression When(decimal whenConstant, SqlExpression then)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), then));
            return this;
        }

        public SqlCaseWhenExpression When(decimal whenConstant, int then)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), Sql.Const(then)));
            return this;
        }

        public SqlCaseWhenExpression When(decimal whenConstant, double then)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), Sql.Const(then)));
            return this;
        }

        public SqlCaseWhenExpression When(decimal whenConstant, float then)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), Sql.Const(then)));
            return this;
        }

        public SqlCaseWhenExpression When(decimal whenConstant, decimal then)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), Sql.Const(then)));
            return this;
        }

        public SqlCaseWhenExpression When(decimal whenConstant, string then)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), Sql.Const(then)));
            return this;
        }
        #endregion

        #region (string, ...)
        public SqlCaseWhenExpression When(string whenConstant, SqlExpression then)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), then));
            return this;
        }

        public SqlCaseWhenExpression When(string whenConstant, int then)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), Sql.Const(then)));
            return this;
        }

        public SqlCaseWhenExpression When(string whenConstant, double then)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), Sql.Const(then)));
            return this;
        }

        public SqlCaseWhenExpression When(string whenConstant, float then)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), Sql.Const(then)));
            return this;
        }

        public SqlCaseWhenExpression When(string whenConstant, decimal then)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), Sql.Const(then)));
            return this;
        }

        public SqlCaseWhenExpression When(string whenConstant, string then)
        {
            _whenThenExpressions.Add((Sql.Const(whenConstant), Sql.Const(then)));
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
