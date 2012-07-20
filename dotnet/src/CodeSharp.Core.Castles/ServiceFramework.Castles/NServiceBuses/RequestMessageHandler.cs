using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NServiceBus;
using CodeSharp.ServiceFramework.Interfaces;
using System.Diagnostics;

namespace CodeSharp.ServiceFramework.NServiceBuses
{
    /// <summary>
    /// 请求消息处理
    /// <remarks>不对异常或失败进行处理，失败则进入重试或丢弃，不向请求端进行反馈</remarks>
    /// </summary>
    public class RequestMessageHandler : IHandleMessages<RequestMessage>
    {
        internal static readonly string HEADER = "result";
        /// <summary>
        /// 获取NServiceBus实例
        /// </summary>
        public IBus Bus { get; set; }
        private Endpoint _endpoint;
        private ILog _log;
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="endpoint"></param>
        public RequestMessageHandler(Endpoint endpoint)
            : this(endpoint
            , endpoint.Resolve<ILoggerFactory>()) { }
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="factory"></param>
        public RequestMessageHandler(Endpoint endpoint, ILoggerFactory factory)
        {
            this._endpoint = endpoint;
            this._log = factory.Create(typeof(RequestMessageHandler));
        }
        /// <summary>
        /// 处理消息
        /// </summary>
        /// <param name="message"></param>
        public void Handle(RequestMessage message)
        {
            var w = new Stopwatch();
            w.Start();
            var success = false;
            try
            {
                var result = this._endpoint.InvokeSerialized(this.RepairCall(message.Request));
                //TODO:重新考虑reply的事务性
                //var response = Bus.CreateInstance<ResponseMessage>(m => m.Result = result);
                ////利用Header来传输响应 支持大文本传输
                //response.SetHeader(HEADER, response.Result);
                //this.Bus.Reply(response);
                success = true;
            }
            catch (Exception e)
            {
                this._log.Warn(string.Format("异步消息调用异常|{0}.{1}|{2}"
                    , message.Request.Target.Name
                    , message.Request.TargetMethod
                    , message.Request.Target.HostUri), e);
                throw e;
            }
            finally
            {
                try
                {
                    w.Stop();
                    if (w.ElapsedMilliseconds >= 2000)
                        this._log.WarnFormat("耗时调用{0}/{1}.{2}={3}ms，请尽快查明原因，类Job的调用禁止异步投递，请使用JobHost方式"
                            , message.Request.Target.Name
                            , message.Request.TargetMethod
                            , message.Request.Target.HostUri
                            , w.ElapsedMilliseconds);

                    if (success)
                        this._log.InfoFormat("完成异步消息处理：{0}/{1}.{2}|Arguments={3}"
                            , message.Request.Target.HostUri
                            , message.Request.Target.Name
                            , message.Request.TargetMethod
                            , string.Join("$"
                            , message.Request.ArgumentCollection.Select(o => o.Key + "=" + o.Value).ToArray()));
                }
                catch { }
            }
        }

        //修正servicecall中无法使用的配置，如：服务地址
        private ServiceCall RepairCall(ServiceCall call)
        {
            var service = this._endpoint.ServiceTable.Services.FirstOrDefault(o =>
                o.Name.Equals(call.Target.Name)
                && o.AssemblyName.Equals(call.Target.AssemblyName));

            if (service == null) return call;

            //修正地址
            if (service.Configs != null
                && service.Configs.Length > 0
                && !service.Configs.ToList().Exists(o => o == call.Target))
            {
                var old = call.Target.HostUri;
                call.Target.HostUri = service.Configs[0].HostUri;
                this._log.InfoFormat("由于原始调用目标服务地址{0}未在服务列表中找到，自动切换为{1}", old, call.Target.HostUri);
            }

            return call;
        }
    }
}