using EasyJsonToSql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyJsonToSql
{
    public interface IStrategy<T>
    {
        //StrategyData<T> Data { get; }
        TReturn GetBuilder<TReturn>(ProxyData proxyData, SqlConfig setting,T rawData) where TReturn:class;
    }
}
