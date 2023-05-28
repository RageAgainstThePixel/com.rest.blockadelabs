using Utilities.WebRequestRest;

namespace Rest.BlockadeLabs
{
    public abstract class BlockadeLabsBaseEndpoint : BaseEndPoint<BlockadeLabsClient, BlockadeLabsAuthentication, BlockadeLabsSettings>
    {
        protected BlockadeLabsBaseEndpoint(BlockadeLabsClient client) : base(client) { }
    }
}
