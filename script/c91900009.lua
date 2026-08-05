-- Divine Dragon of Obelisk
local s,id=GetID()
function s.initial_effect(c)
    -- Fusion Material / Special Summon Procedure
    c:EnableReviveLimit()
    Fusion.AddProcMix(c,true,true,s.matfilter1,s.matfilter2)
    
    -- Must first be Fusion Summoned by sending the above cards to the GY
    local e0=Effect.CreateEffect(c)
    e0:SetType(EFFECT_TYPE_SINGLE)
    e0:SetProperty(EFFECT_FLAG_CANNOT_DISABLE+EFFECT_FLAG_UNCOPYABLE)
    e0:SetCode(EFFECT_SPSUMMON_CONDITION)
    e0:SetValue(aux.fuslimit)
    c:RegisterEffect(e0)

    local e0a=Effect.CreateEffect(c)
    e0a:SetType(EFFECT_TYPE_FIELD)
    e0a:SetProperty(EFFECT_FLAG_UNCOPYABLE)
    e0a:SetCode(EFFECT_SPSUMMON_PROC)
    e0a:SetRange(LOCATION_EXTRA)
    e0a:SetCondition(s.contactcon)
    e0a:SetTarget(s.contacttg)
    e0a:SetOperation(s.contactop)
    c:RegisterEffect(e0a)

    -- Effect 1: Unaffected by other cards' effects, except Divine-Beast
    local e1=Effect.CreateEffect(c)
    e1:SetType(EFFECT_TYPE_SINGLE)
    e1:SetProperty(EFFECT_FLAG_SINGLE_RANGE)
    e1:SetRange(LOCATION_MZONE)
    e1:SetCode(EFFECT_IMMUNE_EFFECT)
    e1:SetValue(s.efilter)
    c:RegisterEffect(e1)

    -- Effect 2: Destroy 2 monsters in hand/field; send opponent's cards to GY & gain ATK
    local e2=Effect.CreateEffect(c)
    e2:SetDescription(aux.Stringid(id,0))
    e2:SetCategory(CATEGORY_DESTROY+CATEGORY_TOGRAVE+CATEGORY_ATKCHANGE)
    e2:SetType(EFFECT_TYPE_IGNITION)
    e2:SetRange(LOCATION_MZONE)
    e2:SetCountLimit(1)
    e2:SetTarget(s.destg)
    e2:SetOperation(s.desop)
    c:RegisterEffect(e2)
end

-- Official Card IDs
local CARD_OBELISK_TORMENTOR = 10000000
local CARD_BLUE_EYES_WHITE   = 89631139

-- Fusion Material Filters
function s.matfilter1(c,fc,sumtype,tp)
    return c:IsCode(21208154) or c:IsCode(10000000) or c:IsRace(RACE_DIVINE)
end
function s.matfilter2(c,fc,sumtype,tp)
    return c:IsCode(CARD_BLUE_EYES_WHITE)
end

-- /// Send to GY Contact Procedure ///
function s.contactfilter(c,tp)
    return c:IsFaceup() and c:IsAbleToGraveAsCost() and (s.matfilter1(c,nil,SUMMON_TYPE_SPECIAL,tp) or s.matfilter2(c,nil,SUMMON_TYPE_SPECIAL,tp))
end
function s.contactcon(e,c)
    if c==nil then return true end
    local tp=c:GetControler()
    local rg=Duel.GetMatchingGroup(s.contactfilter,tp,LOCATION_MZONE,0,nil,tp)
    return Duel.GetLocationCount(tp,LOCATION_MZONE)>-2
        and aux.SelectUnselectGroup(rg,e,tp,2,2,aux.dabcheck,0)
end
function s.contacttg(e,tp,eg,ep,ev,re,r,rp,c)
    local rg=Duel.GetMatchingGroup(s.contactfilter,tp,LOCATION_MZONE,0,nil,tp)
    local g=aux.SelectUnselectGroup(rg,e,tp,2,2,aux.dabcheck,1,tp,HINTMSG_TOGRAVE,nil,nil,true)
    if #g>0 then
        g:KeepAlive()
        e:SetLabelObject(g)
        return true
    end
    return false
end
function s.contactop(e,tp,eg,ep,ev,re,r,rp,c)
    local g=e:GetLabelObject()
    if g then
        Duel.SendtoGrave(g,REASON_COST+REASON_MATERIAL)
        g:Clear()
    end
end

-- /// Effect 1: Immunity Filter ///
function s.efilter(e,te)
    local tc=te:GetHandler()
    return not (tc:IsRace(RACE_DIVINE) or tc:IsHasCardTarget(e:GetHandler()))
end

-- /// Effect 2: Destroy 2, Send Opponent's Cards to GY, Gain ATK ///
function s.desfilter(c,tp)
    return c:IsType(TYPE_MONSTER) and (c:IsLocation(LOCATION_HAND) or (c:IsLocation(LOCATION_MZONE) and c:IsFaceup()))
end
function s.destg(e,tp,eg,ep,ev,re,r,rp,chk)
    local c=e:GetHandler()
    local g=Duel.GetMatchingGroup(s.desfilter,tp,LOCATION_HAND+LOCATION_MZONE,0,nil,tp)
    if chk==0 then 
        return g:GetCount()>=2 and Duel.IsExistingMatchingCard(aux.TRUE,tp,0,LOCATION_ONFIELD,1,nil)
    end
    local dg=aux.SelectUnselectGroup(g,e,tp,2,2,aux.dabcheck,1,tp,HINTMSG_DESTROY)
    Duel.SetTargetCard(dg)
    Duel.SetOperationInfo(0,CATEGORY_DESTROY,dg,2,tp,LOCATION_HAND+LOCATION_MZONE)
    local og=Duel.GetMatchingGroup(aux.TRUE,tp,0,LOCATION_ONFIELD,nil)
    Duel.SetOperationInfo(0,CATEGORY_TOGRAVE,og,#og,0,0)
end
function s.desop(e,tp,eg,ep,ev,re,r,rp)
    local c=e:GetHandler()
    local dg=Duel.GetTargetCards(e)
    if #dg==2 and Duel.Destroy(dg,REASON_EFFECT)>0 then
        local og=Duel.GetMatchingGroup(aux.TRUE,tp,0,LOCATION_ONFIELD,nil)
        if #og>0 then
            local ct=Duel.SendtoGrave(og,REASON_EFFECT)
            if ct>0 and c:IsRelateToEffect(e) and c:IsFaceup() then
                local e1=Effect.CreateEffect(c)
                e1:SetType(EFFECT_TYPE_SINGLE)
                e1:SetCode(EFFECT_UPDATE_ATTACK)
                e1:SetValue(ct*500)
                e1:SetReset(RESET_EVENT+RESETS_STANDARD+RESET_PHASE+PHASE_END)
                c:RegisterEffect(e1)
            end
        end
    end
end