using System.Globalization;
using Microsoft.Extensions.Configuration;
using Npgsql;
using TruckingSystem.Application.FreightChat;

namespace TruckingSystem.Infrastructure.ExternalServices;

public sealed class FreightVectorStore(IConfiguration config, IFreightEmbedding embedding) : IFreightVectorStore
{
    private async Task<NpgsqlConnection> Open(CancellationToken ct)
    {
        var connection = new NpgsqlConnection(config.GetConnectionString("DefaultConnection"));
        try { await connection.OpenAsync(ct); return connection; }
        catch { await connection.DisposeAsync(); throw; }
    }
    // pgvector accepts its text representation; no vector types leak into Application/Domain.
    private static string Vector(float[] values) => "[" + string.Join(",", values.Select(v => v.ToString("R", CultureInfo.InvariantCulture))) + "]";

    public async Task SynchronizeAsync(IReadOnlyList<TariffDocument> documents, CancellationToken ct)
    {
        await using var connection = await Open(ct);
        foreach (var document in documents)
        {
            await using var check = new NpgsqlCommand("SELECT \"Hash\" FROM \"FreightVectors\" WHERE \"TariffId\"=@id AND \"Model\"='gemini-embedding-001'", connection);
            check.Parameters.AddWithValue("id", document.Id);
            if (await check.ExecuteScalarAsync(ct) as string == document.Hash) continue;
            var vector = await embedding.EmbedAsync(document.Text, false, ct);
            await using var write = new NpgsqlCommand("""
                INSERT INTO "FreightVectors" ("TariffId","PriceListId","Text","Hash","Model","Embedding","IndexedAtUtc")
                VALUES (@id,@list,@text,@hash,'gemini-embedding-001',CAST(@vector AS vector),now())
                ON CONFLICT ("TariffId") DO UPDATE SET "PriceListId"=EXCLUDED."PriceListId", "Text"=EXCLUDED."Text",
                "Hash"=EXCLUDED."Hash", "Model"=EXCLUDED."Model", "Embedding"=EXCLUDED."Embedding", "IndexedAtUtc"=now()
                """, connection);
            write.Parameters.AddWithValue("id", document.Id); write.Parameters.AddWithValue("list", document.PriceListId);
            write.Parameters.AddWithValue("text", document.Text); write.Parameters.AddWithValue("hash", document.Hash);
            write.Parameters.AddWithValue("vector", Vector(vector));
            await write.ExecuteNonQueryAsync(ct);
        }
        await using var delete = new NpgsqlCommand("DELETE FROM \"FreightVectors\" WHERE NOT (\"TariffId\" = ANY(@ids))", connection);
        delete.Parameters.AddWithValue("ids", documents.Select(d => d.Id).ToArray());
        await delete.ExecuteNonQueryAsync(ct);
    }
    public async Task<List<TariffMatch>> SearchAsync(int priceListId, string query, CancellationToken ct)
    {
        var vector = await embedding.EmbedAsync(query, true, ct);
        await using var connection = await Open(ct);
        await using var command = new NpgsqlCommand("""
            SELECT "TariffId","Hash" FROM "FreightVectors" WHERE "PriceListId"=@list AND "Model"='gemini-embedding-001'
            ORDER BY "Embedding" <=> CAST(@vector AS vector), "TariffId" LIMIT 5
            """, connection);
        command.Parameters.AddWithValue("list", priceListId); command.Parameters.AddWithValue("vector", Vector(vector));
        await using var reader = await command.ExecuteReaderAsync(ct);
        var matches = new List<TariffMatch>();
        while (await reader.ReadAsync(ct)) matches.Add(new(reader.GetInt32(0), reader.GetString(1)));
        return matches;
    }
}
