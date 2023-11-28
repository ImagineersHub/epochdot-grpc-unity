using System;
using System.Collections.Generic;
using System.Linq;
using UGrpc.Pipeline.GrpcPipe.V1;


namespace UGrpc
{
    public class UnittestPipeImpl : UGrpcEditorPipeImpl
    {
        private Dictionary<string, System.Type> synPusherAssembles;

        public override Dictionary<string, System.Type> AssemblesMappings
        {
            get
            {
                if (synPusherAssembles == null)
                {
                    synPusherAssembles = new Dictionary<string, Type>(){
                        {"UGrpc.UnitTest",typeof(UnitTest)}
                    };
                    synPusherAssembles = synPusherAssembles.Concat(base.mAssembles).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                }
                return synPusherAssembles;
            }
        }

    }
}